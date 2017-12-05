using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SupportBot.Models
{
    /// <summary>
    /// A class holding a list of answers from QnA maker
    /// </summary>
    [Serializable]
    public class QnAMakerAnswers
    {
        public List<Answer> answers { get; set; }
    }
}