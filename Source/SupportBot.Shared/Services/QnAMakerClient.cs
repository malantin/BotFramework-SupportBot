using SupportBot.Models;
using SupportBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace SupportBot.Services
{
    /// <summary>
    /// A custom client to QnA maker, that hold connections to several QnA maker services. You can query just one or many services for an answer at once.
    /// </summary>
    [Serializable]
    public class QnAMakerClient
    {
        /// <summary>
        /// Holds the QnA services of this client
        /// </summary>
        public List<QnAMakerService> QnAServices { get; set; }

        /// <summary>
        /// Gets the names for all available QnA maker services. Every services has a technology category, name or tag.
        /// </summary>
        public List<string> ServiceNames
        {
            get
            {
                var categories = new List<String>();
                foreach (var item in QnAServices)
                {
                    if (item.DisplayCategory)
                    {
                        categories.Add(item.DisplayName);
                    }
                }
                return categories;
            }
        }

        public QnAMakerClient()
        {
            this.QnAServices = new List<QnAMakerService>();
        }

        public QnAMakerClient(List<QnAMakerService> qnaServices)
        {
            this.QnAServices = qnaServices;
        }

        /// <summary>
        /// Looks for an answer in all available QnA maker services and returns the best one based on the score
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public async Task<String> GenerateBestAnswerAsync(string question)
        {
            List<Answer> answers = new List<Answer>();

            foreach (var qnaMakerService in QnAServices)
            {
                // Generate an answer for each service and add it to a list of answers
                answers.AddRange(await qnaMakerService.GenerateAnswersAsync(question));
            }

            // Sort the answers by score
            var bestAnswers = from a in answers
                             orderby a.score descending
                             select a.answer;

            // We just return the best answer
            var answer = bestAnswers.First();
            return answer;
        }

        /// <summary>
        /// Looks for answers with higher confidence then the theshold in all QnA maker services and returns a list of answers sorted by confidence
        /// </summary>
        /// <param name="threshold"></param>
        /// <param name="question"></param>
        /// <returns></returns>
        public async Task<List<String>> GenerateAnswersAsync(double threshold, string question)
        {
            List<Answer> answers = new List<Answer>();

            foreach (var qnaMakerService in QnAServices)
            {
                // Generate an answer for each service and add it to a list of answers
                answers.AddRange(await qnaMakerService.GenerateAnswersAsync(threshold, question));
            }

            // Sort the answers by score
            var answerStrings = from a in answers
                          orderby a.score descending
                          select a.answer;

            var returnList = answerStrings.ToList<String>();
            return returnList;
        }

        /// <summary>
        /// Looks for the best answer in a QnA maker services specified by the technology category and returns the one with the highest confidence
        /// </summary>
        /// <param name="question"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<String> GenerateBestAnswerForTechnologyAsync(string question, string category)
        {
            List<Answer> answers = new List<Answer>();

            foreach (var qnaMakerClient in QnAServices)
            {
                if (qnaMakerClient.DisplayName == category)
                {
                    answers.AddRange(await qnaMakerClient.GenerateAnswersAsync(question));
                    break;
                }                
            }

            var bestAnswers = from a in answers
                             orderby a.score descending
                             select a.answer;

            var answer = bestAnswers.First();
            return answer;
        }

        /// <summary>
        /// Looks for answers in a QnA maker services specified by the technology category and returns them ordered by confidence
        /// </summary>
        /// <param name="question"></param>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<List<String>> GenerateAnswersForTechnologyAsync(string question, string category)
        {
            List<Answer> answers = new List<Answer>();

            foreach (var qnaMakerClient in QnAServices)
            {
                if (qnaMakerClient.DisplayName == category)
                {
                    answers.AddRange(await qnaMakerClient.GenerateAnswersAsync(question));
                    break;
                }
            }

            var bestAnswers = from a in answers
                             orderby a.score descending
                             select a.answer;

            var returnList = bestAnswers.ToList<String>();
            return returnList;
        }
    }
}