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
    /// This dialog presents the user with a list of bot capabilities that are handled in child dialogs
    /// </summary>
    [Serializable]
    public class CategoriesDialog : IDialog<string>
    {

        public Task StartAsync(IDialogContext context)
        {
            // We put together a list a bot capabilities here. They are hard coded for now and need to be changed here and in the message controller.
            List<string> choices = new List<string>() { Resources.BotTexts.TechnicalQuestion, Resources.BotTexts.SearchWikipediaTerm, Resources.BotTexts.SearchThesaurusTerm, Resources.BotTexts.TranslateTerm };

            // We show a prompt and ask the user for a category choice
            PromptDialog.Choice(context, AfterCategorySelectedAsync, choices, Resources.BotTexts.CategorySelectionQuestion, Resources.BotTexts.PleaseUseButtons);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Evaluates the user's answer and call the right child dialogs
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result">The result of the promt dialog</param>
        /// <returns></returns>
        private async Task AfterCategorySelectedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var message = await result;

            if (message == Resources.BotTexts.SearchThesaurusTerm)
            {
                context.Call(new ThesaurusDialog(), AfterChildDialogFinished);
            }
            else if (message == Resources.BotTexts.SearchWikipediaTerm)
            {
                context.Call(new WikiDialog(), AfterChildDialogFinished);
            }
            else if (message == Resources.BotTexts.TranslateTerm)
            {
                context.Call(new TranslateDialog(), AfterChildDialogFinished);
            }
            else
            {
                context.Call(new TechnologiesDialog(), AfterChildDialogFinished);
            }
        }

        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(String.Empty);
            return Task.CompletedTask;
        }
    }
}