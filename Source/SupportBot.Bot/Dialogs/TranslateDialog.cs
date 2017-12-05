using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Net.Http;
using System.Collections.Generic;
using System.Web;
using System.Xml;
using Resources;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// Translates user input into different languages usingbing translatr
    /// TODO Needs some refactoring
    /// </summary>
    [Serializable]
    public class TranslateDialog : IDialog<object>
    {
        // The bing translator token
        private string translatorToken;
        // The language to translate to using country codes like de-de, en-us
        private string translationLanguage;
        // The language as a string
        private string language;

        public async Task StartAsync(IDialogContext context)
        {
            // Try to get the translator token from cognitive services required to use bing translator
            try
            {
                await GetTokenAsync();
            }
            catch (Exception)
            {
                await context.PostAsync(BotTexts.TechnicalProblemWithService);
            }

            // Let the user pick a language to translate
            ChoseLanguage(context);
        }

        /// <summary>
        /// Gets the token for using the translator API
        /// More on http://docs.microsofttranslator.com/oauth-token.html
        /// You will need a key for the Microsoft translator API: https://azure.microsoft.com/en-us/services/cognitive-services/translator-text-api
        /// TODO Extract into a translator service
        /// </summary>
        /// <returns></returns>
        private async Task GetTokenAsync()
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri("https://api.cognitive.microsoft.com/sts/v1.0/")
            };

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "");

            var content = new StringContent("{}");
            //content.Headers.Add("Content-Type", "application/json");

            var response = await client.PostAsync("issueToken", content);

            translatorToken = await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Displays a list of languages to translate to. Currently the list is hard coded here.
        /// </summary>
        /// <param name="context"></param>
        private void ChoseLanguage(IDialogContext context)
        {
            //Initiate a dialog and let the user chose between different languages to translate to
            PromptDialog.Choice(context, SetLanguage,
                new List<string>
                {
                    BotTexts.English, BotTexts.French, BotTexts.Spanish, BotTexts.Italian
                },
                BotTexts.TranslationLanguageQuestion);
        }

        /// <summary>
        /// Processes the user input from the promt choice and maps the input to the language codes
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task SetLanguage(IDialogContext context, IAwaitable<string> result)
        {
            language = await result;

            if (language == BotTexts.English)
            {
                translationLanguage = "en";
            }
            else if (language == BotTexts.Spanish)
            {
                translationLanguage = "es";
            }
            else if (language == BotTexts.French)
            {
                translationLanguage = "fr";
            }
            else if (language == BotTexts.Italian)
            {
                translationLanguage = "it";
            }

            // Ask the user for the text to translate
            await GetTranslationTextAsync(context);
        }

        /// <summary>
        /// Asks the user for the text to translate
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task GetTranslationTextAsync(IDialogContext context)
        {
            await context.PostAsync(String.Format(BotTexts.WhatToTranslateQuestion, language));
            context.Wait(TranslateAsync);
        }

        /// <summary>
        /// Translates through the Microsoft Translator API
        /// More can be found at http://docs.microsofttranslator.com/text-translate.html#/
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task TranslateAsync(IDialogContext context, IAwaitable<object> result)
        {
            var typingMessage = context.MakeMessage();
            typingMessage.Type = ActivityTypes.Typing;
            await context.PostAsync(typingMessage);

            var activity = await result as Activity;

            var translationText = activity.Text;

            if (translatorToken == null)
            {
                await GetTokenAsync();
            }

            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {translatorToken}");
            client.DefaultRequestHeaders.Add("Accept", "application/xml");

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["text"] = translationText;
            query["to"] = translationLanguage;
            query["category"] = "generalnn";

            var builder = new UriBuilder("https://api.microsofttranslator.com/v2/http.svc/Translate")
            {
                Query = query.ToString()
            };

            try
            {
                var response = await client.GetAsync(builder.ToString());

                // If the service returned a statuscode OK, we post the translation to the dialog, if not we inform the user
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var text = await response.Content.ReadAsStringAsync();

                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(text);

                    var translatedText = doc.FirstChild.InnerText;

                    await context.PostAsync(BotTexts.TextFromTranslation);
                    await context.PostAsync(translatedText);
                }
                // Tell the user that the text could not be translated
                else await context.PostAsync(BotTexts.CouldNotTranslateText);
            }
            catch (Exception)
            {
                // Catch problems and tell the user that that thre has been a problem with the service
                await context.PostAsync(BotTexts.TechnicalProblemWithService);
            }

            // Ask the user whether to translate more text and if the same language is supposed to be used
            List<string> choices = new List<string>() { BotTexts.TranslateInDifferentLanguageOption, BotTexts.TranslateSameLanguageOption, BotTexts.No };

            PromptDialog.Choice(context, AfterTranslationSelectedAsync, choices, BotTexts.AfterTranslationQuestion, BotTexts.PleaseUseButtons);
        }

        /// <summary>
        /// Evaluates the users answers on whether to translate another text and either lets the user pick a new language or translate more text
        /// </summary>
        /// <param name="context"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private async Task AfterTranslationSelectedAsync(IDialogContext context, IAwaitable<string> result)
        {
            var choice = await result;

            // If the next translation will be into another language let the user pick the language
            if(choice == BotTexts.TranslateInDifferentLanguageOption)
            {
                ChoseLanguage(context);
            }
            // If the language stays the same ask for the new text to translate.
            else if(choice == BotTexts.TranslateSameLanguageOption)
            {
                await GetTranslationTextAsync(context);
            }
            else
            {
                context.Done(context);
            }
        }
    }
}