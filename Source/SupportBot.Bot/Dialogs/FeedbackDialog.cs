using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using SupportBot.Services;
using SupportBot.Models;
using System.Collections.Generic;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog is used to gather user feedback on the answer the bot provided for a question.
    /// The question and answer is forwarded to the dialog.
    /// </summary>
    [Serializable]
    public class FeedbackDialog : IDialog<object>
    {
        public int FeedbackRating { get; set; }
        public string FeedbackVerbatim { get; set; }
        public string Question { get; set; }
        public string Answer { get; set; }

        public Task StartAsync(IDialogContext context)
        {
            FeedbackRating = 0;
            FeedbackVerbatim = String.Empty;
            Question = String.Empty;
            Answer = String.Empty;

            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Processes the message posted to the dialog, the message string contains values for the question posed and the answer received if available
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // Split the message string to find out whether it contains question and anser or just the question.
            if (activity.Text.Contains(";"))
            {
                Question = activity.Text.Split(';')[0];
                Answer = activity.Text.Split(';')[1];
            }
            else
            {
                Question = activity.Text;
                Answer = String.Empty;
            }

            // Ask the user for feedback
            await context.PostAsync(Resources.BotTexts.FeedbackIntro);

            string[] choices = new string[] { Resources.BotTexts.Yes, Resources.BotTexts.No };

            // Add the pattern for localized strings
            var pattern = new string[2][];
            pattern[PromptDialog.PromptConfirm.Yes] = new string[] { Resources.BotTexts.Yes };
            pattern[PromptDialog.PromptConfirm.No] = new string[] { Resources.BotTexts.No };

            //PromptDialog.Choice(context, AfterCategorySelectedAsync, choices, Resources.BotTexts.GiveFeedbackQuesiton, Resources.BotTexts.PleaseUseButtons);          
            PromptDialog.Confirm(context, AfterCategorySelectedAsync, Resources.BotTexts.GiveFeedbackQuesiton, Resources.BotTexts.PleaseUseButtons,3,PromptStyle.Auto,choices,pattern);
        }

        // Get the user response and either get feedback or end the dialog
        private async Task AfterCategorySelectedAsync(IDialogContext context, IAwaitable<bool> result)
        {
            var confirm = await result;

            if (confirm)
            {
                await GetFeedbackAsync(context);
            }
            else
            {
                await context.PostAsync(Resources.BotTexts.NoFeedbackReply);
                context.Done(string.Empty);
            }
        }

        // Ask the user for a rating
        // TODO Refactor as Prompt
        private async Task GetFeedbackAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.BotTexts.ExperienceRatingQuestion);
            context.Wait(RatingReceivedAsync);
        }

        // Evaluate the answer and either continue to verbatim or reprompt
        // TODO Refactor
        private async Task RatingReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            if (int.TryParse(activity.Text, out int feedbackRating))
            {
                if (feedbackRating > 0 && feedbackRating < 10)
                {
                    FeedbackRating = feedbackRating;
                    await GetVerbatimAsync(context);
                }
                else
                {
                    await context.PostAsync(Resources.BotTexts.FeedbackNoWrongNumber);
                    await GetFeedbackAsync(context);
                }
            }
            else
            {
                await context.PostAsync(Resources.BotTexts.FeedbackNoWrongNumber);
                await GetFeedbackAsync(context);
            }
        }

        // Get verbatim feedback from user
        private async Task GetVerbatimAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.BotTexts.FeedbackQuestion);
            context.Wait(FeedBackReceivedAsync);
        }

        /// <summary>
        /// Process the verbatim feedback and save it through the logging service
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task FeedBackReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            FeedbackVerbatim = activity.Text;

            LoggingService logger = new LoggingService();

            // Try to save the user feedback to the database via the logging service and end the dialog
            try
            {
                await logger.SaveUserFeedback(Question, Answer, FeedbackVerbatim, FeedbackRating);
                await context.PostAsync(Resources.BotTexts.FeedbackSaved);
            }
            catch (Exception)
            {
                await context.PostAsync(Resources.BotTexts.FeedbackNotSaved);
            }
            finally
            {
                context.Done(String.Empty);
            }
        }
    }
}