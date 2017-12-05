using Newtonsoft.Json.Linq;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace SupportBot.Services
{
    /// <summary>
    /// A helper class to get synonyms from OpenThesaurus.de
    /// </summary>
    public class ThesaurusService
    {
        /// <summary>
        /// Gets a list of synonyms for a term
        /// </summary>
        /// <param name="queryTerm"></param>
        /// <returns></returns>
        public static async Task<string> GetSynonyms(string queryTerm)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var query = $"https://www.openthesaurus.de/synonyme/";

                    var searchTerm = WebUtility.HtmlEncode(queryTerm);

                    client.BaseAddress = new Uri(query);

                    string json = await client.GetStringAsync($"search?q={queryTerm}&format=application/json");

                    JObject o = JObject.Parse(json);

                    var synSets = o["synsets"];

                    var returnString = String.Empty;

                    foreach (JObject categoryObject in synSets)
                    {
                        JArray categoryArray = (JArray)categoryObject["categories"];
                        if (categoryArray.Count > 0)
                        {
                            var categoryValue = (string)categoryArray[0];
                            if (categoryValue != string.Empty) returnString += $"**{categoryValue}**: ";
                        }

                        foreach (JObject termObject in categoryObject["terms"])
                        {
                            returnString += (string)termObject["term"];
                            if (termObject != categoryObject["terms"].Last)
                            {
                                returnString += " / ";
                            }

                        }

                        returnString += "\n\n";
                    }
                    if (returnString != string.Empty)
                    {
                        return returnString;
                    }
                    else return Resources.BotTexts.NoThesaurusReply;

                }
            }
            catch
            {
                // If there are problems with the service, let the user know
                return Resources.BotTexts.TechnicalProblemWithService;
            }
        }
    }
}