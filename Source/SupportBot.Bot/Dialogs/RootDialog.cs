using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This is the root dialog that is call by the messages controller
    /// </summary>
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        // When we receive our first message, we forward it to our root LUIS dialogs, that will try to determine the users intent.
        // The message is either the content of one of the buttons or a random string the user typed.
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            // We forward the current (message) activity to our LUIS dialog for evaluation
            await context.Forward(new RootLuisDialog(), DialogCompleted, activity);
        }

        // When the child dialog is completed, we send a final message to the user and wait for new input.
        // New input will restart the dialog from the beginning
        private async Task DialogCompleted(IDialogContext context, IAwaitable<object> result)
        {
            await context.PostAsync(Resources.BotTexts.RootDialogCompleted);
            context.Wait(MessageReceivedAsync);
        }
    }
}