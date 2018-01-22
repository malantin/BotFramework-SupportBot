using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SupportBot.Models
{
    public class SupportBotContext : DbContext
    {
        // You can add custom code to this file. Changes will not be overwritten.
        // 
        // If you want Entity Framework to drop and regenerate your database
        // automatically whenever you change your model schema, please use data migrations.
        // For more information refer to the documentation:
        // http://msdn.microsoft.com/en-us/data/jj591621.aspx
    
        public SupportBotContext() : base("name=SupportBotContext")
        {
        }

        public System.Data.Entity.DbSet<SupportBot.Models.QnAPair> QnAPair { get; set; }
        public System.Data.Entity.DbSet<SupportBot.Services.QnAMakerService> QnAMakerService { get; set; }
        public System.Data.Entity.DbSet<SupportBot.Models.QuestionLogEntry> QuestionLogEntry { get; set; }
        public System.Data.Entity.DbSet<SupportBot.Models.UserFeedback> UserFeedback { get; set; }
    }
}
