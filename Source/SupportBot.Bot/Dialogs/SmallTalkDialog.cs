using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog handles the small talk like "I love you""
    /// </summary>
    [Serializable]
    public class SmallTalkDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles the users input and sends it to the QnA Maker database for handling
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // We create a new QnAMaker service for handling the small talk. This is hard coded here for now
            // TODO Remove before publishing the source code publicly
            var smallTalkService = new QnAMakerService("", "", SupportBot.Models.ServiceRegion.WestUS, "Small Talk");

            try
            {
                // If the service returns and answer we post it to the conversation, we use a standard confidence of 0.0 to get an answer
                var answer = await smallTalkService.GenerateAnswerStringAsync(activity.Text);
                if (answer != null && answer != string.Empty && answer != "No good match found in the KB")
                {
                    await context.PostAsync(answer);
                }
                else
                {
                    // If we don't have an appropriate response we provide a default answer
                    await context.PostAsync(Resources.BotTexts.NoAnswerSmallTalk);
                }
            }
            catch (Exception e)
            {
                //TODO Remove
                await context.PostAsync(e.Message);

                // Tell the user if the QnA maker service is not available or any other problem occors
                await context.PostAsync(Resources.BotTexts.DatebaseNotAvailable);
            }
            finally
            {
                // End the dialog
                context.Done(String.Empty);
            }

        }

        // End the dialog after a child dialog finishes
        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<string> result)
        {
            context.Done(String.Empty);
            return Task.CompletedTask;
        }

    }
}