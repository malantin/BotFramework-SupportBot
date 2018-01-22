using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Collections.Generic;
using SupportBot.Services;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog tages the first message from the user and sends the text to LUIS. LUIS tries to detect the intent here. 
    /// The attribute LuisModel takes the ID of the services, the key and information about API level, region, etc.
    /// Please keep in mind that there a different LUIS regions and that you have a production and staging deployment of your LUIS service
    /// </summary>
    [LuisModel("", "", LuisApiVersion.V2, "westeurope.api.cognitive.microsoft.com")]
    [Serializable]
    public class RootLuisDialog : LuisDialog<object>
    {
        // If no intent is determined we forward the user input to our small talk dialog
        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.Forward(new SmallTalkDialog(), AfterChildDialogFinished, message);
        }

        // If we detect a greeting we call the greeting dialog
        [LuisIntent("Greeting")]
        public Task Greeting(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new GreetingDialog(), AfterChildDialogFinished);
            return Task.CompletedTask;
        }

        // If the user want to know about the bot's capabilities we call the technologies dialog
        [LuisIntent("Categories")]
        public Task Categories(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new TechnologiesDialog(), AfterChildDialogFinished);
            return Task.CompletedTask;
        }

        // If the user want to know about the bot's capabilities we call the technologies dialog
        [LuisIntent("Overview")]
        public Task Overview(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new TechnologiesDialog(), AfterChildDialogFinished);
            return Task.CompletedTask;
        }

        // If the user aked a question we forward the user input to our QnA dialog
        [LuisIntent("Question")]
        public async Task Question(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.Forward(new QnADialog(), AfterChildDialogFinished, message);
        }

        // If the user wants to look up a word in the encyclopedia we call the wiki dialog
        [LuisIntent("Wiki")]
        public Task Wiki(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new WikiDialog(), WikiDialogFinished);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is being called after the Wiki dialog completes, asks the user if they want to look up another term
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Task WikiDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            PromptDialog.Choice(context, RepeatWikiDialogQuestion,
                new List<string>
                {
                    Resources.BotTexts.Yes, Resources.BotTexts.No
                },
                Resources.BotTexts.AnotherWikiSearchQuestion);
            return Task.CompletedTask;
        }

        // Checks if the user wants to look up another term, if yes another Wiki dialog is called
        private async Task RepeatWikiDialogQuestion(IDialogContext context, IAwaitable<string> result)
        {
            var repeatAnswer = await result;

            if (repeatAnswer == Resources.BotTexts.Yes)
            {
                context.Call(new WikiDialog(), WikiDialogFinished);
            }
            else if (repeatAnswer == Resources.BotTexts.No)
            {
                context.Done(String.Empty);
            }
        }

        // If the user wants to look up a synonym, the Thesaurus dialog is called
        [LuisIntent("Synonym")]
        public Task Synonym(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new ThesaurusDialog(), ThesaurusDialogFinished);
            return Task.CompletedTask;
        }

        /// <summary>
        /// Is being called after the Thesaurus dialog completes, asks the user if they want to look up another term
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private Task ThesaurusDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            PromptDialog.Choice(context, RepeatSynonymDialogQuestion,
                new List<string>
                {
                    Resources.BotTexts.Yes, Resources.BotTexts.No
                },
                Resources.BotTexts.AnotherWikiSearchQuestion);
            return Task.CompletedTask;
        }

        // If the user wants to look a anonther synonym the Thesaurus dialog is called once again
        private async Task RepeatSynonymDialogQuestion(IDialogContext context, IAwaitable<string> result)
        {
            var repeatAnswer = await result;

            if (repeatAnswer == Resources.BotTexts.Yes)
            {
                context.Call(new ThesaurusDialog(), ThesaurusDialogFinished);
            }
            else if (repeatAnswer == Resources.BotTexts.No)
            {
                context.Done(String.Empty);
            }
        }

        // If a translation intent is detected the Translate Dialog is called
        [LuisIntent("Translation")]
        public Task Translation(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            context.Call(new TranslateDialog(), AfterChildDialogFinished);
            return Task.CompletedTask;
        }

        // If the user asks for the lunch menu, we can't help right now and end the dialog
        [LuisIntent("LunchMenu")]
        public async Task LunchMenu(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync("Lunch intent not implemented yet");
            context.Done(String.Empty);
        }

        // If the user asks for the weather, we can't help right now and end the dialog
        [LuisIntent("Weather")]
        public async Task Weather(IDialogContext context, IAwaitable<IMessageActivity> activity, LuisResult result)
        {
            var message = await activity;
            await context.PostAsync("Weather intent not implemented yet");
            context.Done(String.Empty);
        }

        // We use this method to end the dialog after a child dialog finishes
        private Task AfterChildDialogFinished(IDialogContext context, IAwaitable<object> result)
        {
            context.Done(String.Empty);
            return Task.CompletedTask;
        }
    }
}