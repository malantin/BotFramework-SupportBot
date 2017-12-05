using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Linq;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Autofac;
using System.Collections.Generic;

namespace SupportBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                await Conversation.SendAsync(activity, () => new Dialogs.RootDialog());
            }
            else
            {
                await HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private async Task HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels

                // When the bot enters the conversation, post a first message to provide the user some information on the bot's capabilities
                // We filter for the name of the new user in the chat, so we need to enter the possibles names of the bot here
                var botnames = new string[] { "Bot", "SupportBot", "Support" };
                // What is the name of the user who entered the conversation
                var newuser = message.MembersAdded?.ElementAt(0).Name;
                // Did the bot enter the conversation?
                if (botnames.Contains(newuser))
                {
                    // Create a message activity to send to the conversation
                    Activity m = message.CreateReply();
                    m.Text = Resources.BotTexts.WelcomeTextCategories;

                    // We want to provide buttons for the user to pick between the different bot capabilities. 
                    // To create buttons, we use a Hero Card with a number of buttons.
                    m.AddHeroCard<string>(string.Empty, new List<string>() { Resources.BotTexts.TechnicalQuestion, Resources.BotTexts.SearchWikipediaTerm, Resources.BotTexts.SearchThesaurusTerm, Resources.BotTexts.TranslateTerm });

                    // We need a client to send the message to the conversation, so we create a new one from the the current scope. 
                    // This is a new client, which sends to the user. It is decoupled from the main dialog
                    using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                    {
                        var client = scope.Resolve<IConnectorClient>();
                        // Send the message to the conversation
                        await client.Conversations.ReplyToActivityAsync(m);
                    }
                }
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }
        }
    }
}