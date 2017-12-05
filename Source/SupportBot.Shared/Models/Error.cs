using System;
using System.Collections.Generic;
using System.Text;

namespace SupportBot.Models
{
    /// <summary>
    /// A class representing error information from QnA maker
    /// </summary>
    [Serializable]
    public class Error
    {
        public string code { get; set; }
        public string message { get; set; }
    }
}
