using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;
using SupportBot.Models;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// Displays the available technology categories for technical questions
    /// </summary>
    [Serializable]
    public class TechnologiesDialog : IDialog<string>
    {
        QnAMakerClient client = new QnAMakerClient();

        public Task StartAsync(IDialogContext context)
        {
            try
            {
                // Load QnA Services from Database
                SupportBotContext db = new SupportBotContext();
                var qnaServicesSet = db.Set<QnAMakerService>();
                foreach (var item in qnaServicesSet)
                {
                    client.QnAServices.Add(item);
                }

                // Display a promt choise with all the different technologies available
                PromptDialog.Choice(context, AfterTechnologySelectedAsync, client.ServiceNames, Resources.BotTexts.CategorySelectionQuestion, Resources.BotTexts.PleaseUseButtons);

            }
            catch (Exception)
            {
                // If the database if not available notify the user
                context.PostAsync(Resources.BotTexts.DatebaseNotAvailable);
                context.Done(String.Empty);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Takes the users technology selection and forwards it to the QnA dialog to answer a question
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterTechnologySelectedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var selection = await result;
            IMessageActivity messageActivity = Activity.CreateMessageActivity();
            messageActivity.Text = selection;
            await context.Forward(new QnADialog(), AfterChildDialogFinished, messageActivity);
        }

        /// <summary>
        /// Ends the dialog after a child dialog conpleted
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(String.Empty);
            return Task.CompletedTask;
        }
    }
}