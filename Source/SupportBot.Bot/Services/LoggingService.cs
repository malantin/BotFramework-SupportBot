using SupportBot.Models;
using System.Threading.Tasks;

namespace SupportBot.Services
{
    /// <summary>
    /// This helper class is used to logg information to the database using Entity Framework
    /// </summary>
    public class LoggingService
    {
        private SupportBotContext db = new SupportBotContext();

        /// <summary>
        /// Saves questions to the database that could not be answered
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public async Task SaveUnansweredQuestion(string question)
        {
            db.QnAPair.Add(new QnAPair(question));
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Saves every question that is send to the bot
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public async Task LogQuestion(string question)
        {
            var q = new QuestionLogEntry(question);
            db.QuestionLogEntry.Add(q);
            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Saves user feedback for answered questions
        /// </summary>
        /// <param name="question"></param>
        /// <param name="answer"></param>
        /// <param name="feedbackText"></param>
        /// <param name="rating"></param>
        /// <returns></returns>
        public async Task SaveUserFeedback(string question, string answer, string feedbackText, int rating)
        {
            db.UserFeedback.Add(new UserFeedback(question, answer, feedbackText, rating));
            await db.SaveChangesAsync();
        }
    }
}