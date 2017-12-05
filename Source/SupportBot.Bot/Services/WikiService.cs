using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SupportBot.Services
{
    /// <summary>
    /// A helper class to connect to Wikipedia and look up abstracts from the Wikipedia article
    /// </summary>
    public class WikiService
    {
        /// <summary>
        /// Gets the abstract the Wikipedia article for a term
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        public static async Task<string> GetAbstract(string term)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var query = $"https://en.wikipedia.org/w/";

                    var searchTerm = term.Replace(" ", "%20");

                    client.BaseAddress = new Uri(query);

                    string json = await client.GetStringAsync($"api.php?format=json&action=query&prop=extracts&exintro=&explaintext=&titles={searchTerm}");

                    JObject o = JObject.Parse(json);

                    var wikiabstract = (string)o["query"]["pages"].First.First["extract"];

                    if (wikiabstract == string.Empty || wikiabstract == null)
                    {
                        return Resources.BotTexts.NoWikiReply;
                    }

                    return wikiabstract;
                }
            }
            catch
            {
                return Resources.BotTexts.TechnicalProblemWithService;
            }
        }
    }
}