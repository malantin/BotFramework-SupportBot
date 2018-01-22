using Microsoft.Bot.Builder.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Bot.Connector;
using System.Threading.Tasks;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This dialog provides help options, whenever the user mentions the world help
    /// </summary>
    [Serializable]
    public class HelpDialog : IDialog<object>
    {
        [NonSerialized]
        private Activity activity;

        public HelpDialog(Activity activity)
        {
            var heroCard = new HeroCard
            {
                Title = Resources.BotTexts.Help,
                Text = Resources.BotTexts.NeedHelpQuestion,
                Buttons = new List<CardAction> {
                    new CardAction(ActionTypes.OpenUrl, "Get Developer Help", value: "https://stackoverflow.com/questions/tagged/botframework"),
                    new CardAction(ActionTypes.OpenUrl, "Learn about Bot Service", value: "https://docs.microsoft.com/bot-framework")
                }
            };

            var reply = activity.CreateReply();

            reply.Attachments.Add(heroCard.ToAttachment());

            this.activity = reply;
        }

        public async Task StartAsync(IDialogContext context)
        {
            if (activity != null)
            {
                await context.PostAsync(activity);

            }

            context.Done<object>(null);
        }
    }
}