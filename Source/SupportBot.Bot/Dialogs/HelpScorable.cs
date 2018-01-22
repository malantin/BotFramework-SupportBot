using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SupportBot.Dialogs
{
    /// <summary>
    /// This is the Scorable (https://blog.botframework.com/2017/07/06/scorables/) for the help dialog
    /// </summary>
    public class HelpScorable : ScorableBase<IActivity, string, double>
    {
        private readonly IDialogStack stack;

        public HelpScorable(IDialogStack stack)
        {
            SetField.NotNull(out this.stack, nameof(stack), stack);
        }

        protected override async Task<string> PrepareAsync(IActivity activity, CancellationToken token)
        {
            var message = activity as IMessageActivity;

            if (message != null && !string.IsNullOrWhiteSpace(message.Text))
            {
                var msg = message.Text.ToLowerInvariant();

                if (msg.Contains(Resources.BotTexts.Help.ToLowerInvariant()))
                {
                    return message.Text;
                }
            }

            return null;
        }

        protected override bool HasScore(IActivity item, string state)
        {
            return state != null;
        }

        protected override double GetScore(IActivity item, string state)
        {
            return state != null ? 1.0 : 0.0;
        }

        protected override async Task PostAsync(IActivity item, string state, CancellationToken token)
        {
            var message = item as IMessageActivity;

            var dialog = new HelpDialog((Activity) message);

            var interruption = dialog.Void(stack);

            stack.Call(interruption, null);
        }

        protected override Task DoneAsync(IActivity item, string state, CancellationToken token)
        {
            return Task.CompletedTask;
        }
    }
}