using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using SupportBot.Services;
using SupportBot.Models;
using System.Collections.Generic;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog is the heart of the bot, as it is taking care of answering the technical questions
    /// </summary>
    [Serializable]
    public class QnADialog : IDialog<object>
    {
        // The client for accessing the QnA maker services
        QnAMakerClient client = new QnAMakerClient();
        // The list of answers received from the QnAMakerClient instance
        List<String> Answers = new List<string>();
        // The users question
        String Question = String.Empty;
        // The technology category of the users questions if available
        String Category = String.Empty;

        // A switch useds to turn the collection of user feedback after each question on or off
        bool RequestFeedback = true;

        public Task StartAsync(IDialogContext context)
        {
            try
            {
                // Load QnA Services from database
                // Create the database context first
                SupportBotContext db = new SupportBotContext();
                // Load the available QnA services sets from the database
                var qnaServicesSet = db.Set<QnAMakerService>();
                // Add the QnA service references to the QnA Maker Client 
                foreach (var item in qnaServicesSet)
                {
                    client.QnAServices.Add(item);
                }

                context.Wait(MessageReceivedAsync);
            }
            catch (Exception e)
            {
                //TODO Remove
                context.PostAsync(e.Message);
                // If the database isn't available or if there is any other problem, let the user know and end the dialog
                context.PostAsync(Resources.BotTexts.DatebaseNotAvailable);
                context.Done(String.Empty);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Evaluates user input and checks if technology category or a question was sent
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            // Get the input
            var activity = await result as Activity;
            var userInput = activity.Text;

            // Check if the input was one of the technology categories available
            if (client.ServiceNames.Contains(userInput))
            {
                // Set the category
                Category = userInput;

                // Ask the user for the question
                await context.PostAsync(String.Format(Resources.BotTexts.QnAQuestion, Category));
                context.Wait(QuestionReceivedAsync);
            }
            else
            {
                // If the input to the dialog was a question we are sending this question to all QnA maker services
                await SearchAllQnADatabases(context, userInput);
            }
        }

        /// <summary>
        /// Receives a question and checks for the answer
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task QuestionReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;
            Question = activity.Text;
            // use the client to connect to QnA maker and get the best answer
            var answersTask = client.GenerateAnswersForTechnologyAsync(Question, Category);
            // ask the user to verify the answer provided
            await VerifyAnswers(context, answersTask);
        }

        /// <summary>
        /// Uses the LogginService to log questions asked
        /// </summary>
        /// <returns></returns>
        private async Task LogQuestion()
        {
            var logger = new LoggingService();
            try
            {
                await logger.LogQuestion(Question);
            }
            catch (Exception)
            {
                // TODO Handle the exception
                // Couldn't save question to database
            }
        }

        /// <summary>
        /// Receives the question and queries all QnAMaker services available
        /// </summary>
        /// <param name="context"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        private async Task SearchAllQnADatabases(IDialogContext context, string question)
        {
            // Set the question
            Question = question;

            // Query all services for an answer using als threshold fo 0.25
            var answersTask = client.GenerateAnswersAsync(25.0, Question);

            // Tell the user that it might take some time
            await context.PostAsync(Resources.BotTexts.SearchingDatabases);

            // Send typing activity
            var typingMessage = context.MakeMessage();
            typingMessage.Type = ActivityTypes.Typing;
            await context.PostAsync(typingMessage);

            // Ask the user to verify the answer
            await VerifyAnswers(context, answersTask);
        }

        /// <summary>
        /// Awaits the response and asks the user to verify the answer provided
        /// </summary>
        /// <param name="context"></param>
        /// <param name="answersTask"></param>
        /// <returns></returns>
        private async Task VerifyAnswers(IDialogContext context, Task<List<String>> answersTask)
        {
            // Log question to Database
            await LogQuestion();

            // Wait for the task to complete
            Answers = await answersTask;

            // Check if there has been an answer returned
            if (Answers != null && Answers.Count > 0 && Answers[0] != "No good match found in the KB")
            {
                await context.PostAsync(Resources.BotTexts.FoundQnAAnswer);
                await context.PostAsync(Answers[0]);

                // Ask the user if this answer solves the problem
                // TODO Check if it makes sense to change this to a confirm prompt
                PromptDialog.Choice(context, AfterCorrectAnswerAsync,
                new List<string>
                {
                        Resources.BotTexts.Yes,
                        Resources.BotTexts.No
                },
                Resources.BotTexts.ProblemSolvedQuestion,
                Resources.BotTexts.PleaseUseButtons);
            }
            else
            {
                // Tell the user that we have not found an answer
                await context.PostAsync(Resources.BotTexts.NoQnAReply);
                // Ask the user if we need to create a support ticket
                EscaleToSupportQuestion(context);
                return;
            }
        }

        /// <summary>
        /// Evaluates the users response to the question whether the answer was correct
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterCorrectAnswerAsync(IDialogContext context, IAwaitable<string> result)
        {
            var resultString = await result;

            if (resultString == Resources.BotTexts.Yes)
            {
                await context.PostAsync(Resources.BotTexts.GladToHelp);
                if (RequestFeedback)
                {
                    await RequestFeedbackAsync(context);
                }
                else context.Done(string.Empty);
            }
            // If the answer was not correct, check if we found alternatives and present them to the user
            else if (Answers.Count > 1)
            {
                await context.PostAsync(Resources.BotTexts.FoundMoreAnswers);
                for (int i = 1; i < Answers.Count; i++)
                {
                    await context.PostAsync(Answers[i]);
                }

                // Ask the user if these answers solve answer the question
                PromptDialog.Choice(context, AfterProblemSolvedAsync,
                new List<string>
                {
                        Resources.BotTexts.Yes,
                        Resources.BotTexts.No
                },
                Resources.BotTexts.ProblemSolvedQuestion,
                Resources.BotTexts.PleaseUseButtons);
            }
            // If there are no alternatives ask the user if a support ticket is required
            else
            {
                EscaleToSupportQuestion(context);
            }
        }

        /// <summary>
        /// Asks the user if we need to create a support ticket
        /// </summary>
        /// <param name="context"></param>
        private void EscaleToSupportQuestion(IDialogContext context)
        {
            PromptDialog.Choice(context, AfterSupportQuestionAsync,
                    new List<string>
                    {
                                Resources.BotTexts.Yes,
                                Resources.BotTexts.No
                    },
                    Resources.BotTexts.EscalateToSupportQuestion,
                    Resources.BotTexts.PleaseUseButtons);
        }

        /// <summary>
        /// As the user if one of the answers was correct
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterProblemSolvedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var resultString = await result;

            if (resultString == Resources.BotTexts.Yes)
            {
                await context.PostAsync(Resources.BotTexts.GladToHelp);
                if (RequestFeedback)
                {
                    await RequestFeedbackAsync(context);
                }
                else context.Done(string.Empty);
            }
            // If the user said no, we ask if we need to create a support ticket
            else
            {
                EscaleToSupportQuestion(context);
            }
        }

        /// <summary>
        /// Forwards the question and answer to the feedback dialog
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task RequestFeedbackAsync(IDialogContext context)
        {

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Text = Question;
            if (Answers.Count > 0)
            {
                message.Text += ";" + Answers[0];
            }

            await context.Forward(new FeedbackDialog(), AfterChildDialogFinished, message);
        }

        /// <summary>
        /// If the user needs support this method forwards the question to the support dialog. If not, we ask the user for feedback.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterSupportQuestionAsync(IDialogContext context, IAwaitable<string> result)
        {
            var resultString = await result;

            IMessageActivity message = Activity.CreateMessageActivity();
            message.Text = Category + ";" + Question;

            if (resultString == Resources.BotTexts.Yes)
            {
                await context.Forward(new SupportDialog(), AfterSupportDialogFinished, message);
            }
            else if (RequestFeedback)
            {
                await RequestFeedbackAsync(context);
            }
            else context.Done(string.Empty);
        }

        /// <summary>
        /// Asks for feedback after the support dialog completed and end the dialog
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterSupportDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            if (RequestFeedback)
            {
                await RequestFeedbackAsync(context);
            }
            else context.Done(string.Empty);
        }

        /// <summary>
        /// Ends the dialog after a child dialog has been called
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(string.Empty);
            return Task.CompletedTask;
        }

    }
}