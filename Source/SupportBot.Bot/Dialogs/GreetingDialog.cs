using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;

namespace SupportBot.Dialogs
{
    [Serializable]
    public class GreetingDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            // Greet the user
            await context.PostAsync(CreateGreeting(context));
            // Call the categories dialog after greeting the user
            context.Call(new CategoriesDialog(), AfterChildDialogFinished);
        }

        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<string> result)
        {
            context.Done(String.Empty);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Creates a personalize greeting for the user using information from the context
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private String CreateGreeting(IDialogContext context)
        {
            //return String.Format(Resources.BotTexts.GreetingGeneral, context.MakeMessage().Recipient.Name);
            return "Hi! Nice to meet you.";
        }
    }
}