using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using SupportBot.Services;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog gets user input and looks up information on a term on Wikipedia
    /// </summary>
    [Serializable]
    public class WikiDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            await context.PostAsync(Resources.BotTexts.WikiSearchQuestion);

            context.Wait(MessageReceivedAsync);
                       
        }

        /// <summary>
        /// Takes the user input and uses the WikiService to look up the term in Wikipedia and post the abstract to the user
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            await context.PostAsync(await WikiService.GetAbstract(activity.Text));

            context.Done(string.Empty);
        }
    }
}