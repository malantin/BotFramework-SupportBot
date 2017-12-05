using System;
using System.Collections.Generic;
using System.Text;

namespace SupportBot.Models
{
    /// <summary>
    /// A class holding an error from QnA maker
    /// </summary>
    [Serializable]
    public class QnAMakerError
    {
        public Error error { get; set; }
    }
}
