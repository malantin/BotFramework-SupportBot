using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupportBot.Models
{
    /// <summary>
    /// A class representing an answer from QnA maker. Multiple questions can match to one answer. 
    /// </summary>
    [Serializable]
    public class Answer
    {
        public string answer { get; set; }
        public List<string> questions { get; set; }
        public double score { get; set; }
    }
}