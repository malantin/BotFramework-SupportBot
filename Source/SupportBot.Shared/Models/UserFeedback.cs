using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SupportBot.Models
{
    /// <summary>
    /// User feedback to save in the database
    /// </summary>
    [Serializable]
    [Table("UserFeedback")]
    public class UserFeedback
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Verbatim { get; set; }
        [Required]
        public string Question { get; set; }
        public string Answer { get; set; }
        public DateTime DateTime { get; set; }
        public int Rating { get; set; }

        public UserFeedback(string question, string answer, string feedbackText, int rating)
        {
            this.Question = question;
            this.Answer = answer;
            this.Verbatim = feedbackText;
            this.Rating = rating;
            DateTime = DateTime.UtcNow;
        }
    }
}
