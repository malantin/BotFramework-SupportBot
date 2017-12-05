using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SupportBot.Services;
using SupportBot.Models;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog is used to create support emails and tickets at one point
    /// </summary>
    [Serializable]
    public class SupportDialog : IDialog<object>
    {
        // TODO Set the correct support email address here
        static string SupportEmail = "helpdesk@contoso.com";

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Receive a message and create the the support email and logs the unanswered question
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            var supportStrings = activity.Text.Split(';');

            if (supportStrings.Length > 1)
            {
                string categoryString = supportStrings[0];

                // Check if a category is set
                if (categoryString != string.Empty)
                {
                    categoryString = Resources.BotTexts.Category + ":" + categoryString + "%0D%0A";
                }

                // Get the question from the string
                string question = supportStrings[1];
                
                // Create a logger service
                LoggingService logger = new LoggingService();

                try
                {
                    // Try to log the question to the database
                    await logger.SaveUnansweredQuestion(question);
                    await context.PostAsync(Resources.BotTexts.QuestionSaved);
                }
                catch (Exception)
                {
                    // Tell the user that the question could not be saved
                    await context.PostAsync(Resources.BotTexts.QuestionNotSaved);
                }
                finally
                {
                    // Create the email and send the link to the conversation
                    await context.PostAsync($"{Resources.BotTexts.ClickLinkForSupportMail} [E-Mail](mailto:{SupportEmail}?subject={String.Format(Resources.BotTexts.SupportMailSubject, question)}&body={String.Format(Resources.BotTexts.SupportMailBody, categoryString, question)})");
                }
            }

            context.Done(string.Empty);

        }

    }
}