using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SupportBot.Models
{
    /// <summary>
    /// A log entry to save in the database
    /// </summary>
    [Serializable]
    [Table("QuestionLogEntry")]
    public class QuestionLogEntry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Question { get; set; }
        public DateTime DateTime { get; set; }

        public QuestionLogEntry(string question)
        {
            Question = question;
            DateTime = DateTime.UtcNow;
        }
    }
}
