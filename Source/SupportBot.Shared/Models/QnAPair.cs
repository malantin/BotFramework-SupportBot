using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace SupportBot.Models
{
    /// <summary>
    /// A question and answer pair to save in the database. Used to add new questions to QnA maker.
    /// </summary>
    [Serializable]
    [Table("QnAPair")]
    public class QnAPair
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public DateTime DateAdded { get; set; }
        [Required]
        public string Question { get; set; }
        public string Answer { get; set; }

        public QnAPair()
        {
            DateAdded = DateTime.UtcNow;
        }

        public QnAPair(string question, string answer) : this()
        {
            Question = question;
            Answer = answer;
        }

        public QnAPair(string question) : this()
        {
            Question = question;
            Answer = String.Empty;
        }
    }
}