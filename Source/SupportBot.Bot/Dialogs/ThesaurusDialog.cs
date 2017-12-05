using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog takes input from the user and find synonyms for the input using OpenThesaurus
    /// </summary>
    [Serializable]
    public class ThesaurusDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.BotTexts.ThesaurusSearchQuestion);

            context.Wait(MessageReceivedAsync);

        }

        /// <summary>
        /// Takes the user input and uses the ThesaurusService class to get synonyms
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            await context.PostAsync(await ThesaurusService.GetSynonyms(activity.Text));

            context.Done(string.Empty);
        }
    }
}