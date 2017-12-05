using Newtonsoft.Json;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using SupportBot.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SupportBot.Services
{
    /// <summary>
    /// Provides access to a QnA maker service via API V2
    /// </summary>
    [Serializable]
    [Table("QnAMakerService")]
    public class QnAMakerService
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string Id { get; set; }
        [Required]
        public string ServiceId { get; set; }
        [Required]
        public String BaseAddress { get; set; }
        [Required]
        public string Key { get; set; }
        [Required]
        public string DisplayName { get; set; }
        public bool DisplayCategory { get; set; }
        [NotMapped]
        public string RequestUri { get { return $"knowledgebases/{ServiceId}/generateAnswer"; } }

        public QnAMakerService()
        { }

        /// <summary>
        /// Create a new instance of a service connector, specifying a region and category
        /// </summary>
        /// <param name="serviceId">The ID for the QnA maker service</param>
        /// <param name="key">The Key for the QnA maker Service</param>
        /// <param name="region">The geographic region of the service. Currently just WestUS is supported</param>
        /// <param name="name"></param>
        public QnAMakerService(string serviceId, string key, ServiceRegion region, string name)
        {
            this.DisplayCategory = true;
            this.ServiceId = serviceId;
            this.Key = key;
            this.DisplayName = name;
            switch (region)
            {
                case ServiceRegion.WestUS:
                    this.BaseAddress = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/";
                    break;
                default:
                    this.BaseAddress = "https://westus.api.cognitive.microsoft.com/qnamaker/v2.0/";
                    break;
            }
        }

        /// <summary>
        /// Gets the best answer with no confidence threshold
        /// </summary>
        /// <param name="question">The question</param>
        /// <returns>The best answer</returns>
        public async Task<String> GenerateAnswerStringAsync(string question)
        {
            return await GenerateAnswerStringAsync(0.0, question);
        }

        /// <summary>
        /// Gets the best answer with a specified confidence threshold
        /// </summary>
        /// <param name="threshold">Confidence threshold</param>
        /// <param name="question">The question</param>
        /// <returns></returns>
        public async Task<String> GenerateAnswerStringAsync(double threshold, string question)
        {
            var answers = await GenerateAnswersAsync(threshold, question);
            if (answers != null && answers.Count > 0)
            {
                return answers.First().answer;
            }
            else return null;
        }

        /// <summary>
        /// Gets a list of answers with no confidence threshold, ordered by confidence
        /// </summary>
        /// <param name="question"></param>
        /// <returns></returns>
        public async Task<List<Answer>> GenerateAnswersAsync(string question)
        {
            return await GenerateAnswersAsync(0.0, question);
        }

        /// <summary>
        /// Gets a list of answers with a spefified confidence threshold
        /// </summary>
        /// <param name="threshold">Confidence threshold</param>
        /// <param name="question">The question</param>
        /// <returns></returns>
        public async Task<List<Answer>> GenerateAnswersAsync(double threshold, string question)
        {
            // Try connecting to the QnA maker service
            try
            {
                using (var client = new HttpClient())
                {

                    client.BaseAddress = new Uri(BaseAddress);

                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", Key);

                    var requestBody = $"{{\"question\": \"{question}\",\"top\": 3}}";

                    HttpResponseMessage response;

                    byte[] byteData = Encoding.UTF8.GetBytes(requestBody);

                    using (var content = new ByteArrayContent(byteData))
                    {
                        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                        response = await client.PostAsync(RequestUri, content);
                    }

                    var answersJson = await response.Content.ReadAsStringAsync();

                    List<Answer> answersAboveThreshold = new List<Answer>();

                    // Try do deserialize the answers, if the rate limit was exceeded this might fail
                    try
                    {
                        var qnaanswers = JsonConvert.DeserializeObject<QnAMakerAnswers>(answersJson);
                        foreach (var answer in qnaanswers.answers)
                        {
                            if (answer.score >= threshold && answer.answer != "No good match found in the KB")
                            {
                                answersAboveThreshold.Add(answer);
                            }
                        }
                    }
                    catch
                    {
                        //Catch "Rate Limit Exceeded" errors and let the user know
                        var qnaerror = JsonConvert.DeserializeObject<QnAMakerError>(answersJson);
                        answersAboveThreshold.Add(new Answer() { answer = qnaerror.error.message });
                    }

                    return answersAboveThreshold;
                }
            }
            catch
            {
                //Carch all other exception when accessing the database and let the user know
                return new List<Answer> { new Answer() { score = threshold, answer = $"A problem occured when accessing database {this.DisplayName}." } };
            }

        }
    }
}