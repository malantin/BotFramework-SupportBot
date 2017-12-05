using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SupportBot.Models;
using SupportBot.Services;

namespace SupportBot.Portal.Models
{
    public class SupportBotContext : DbContext
    {
        public SupportBotContext (DbContextOptions<SupportBotContext> options)
            : base(options)
        {
        }

        public DbSet<SupportBot.Models.QnAPair> QnAPair { get; set; }
        public DbSet<SupportBot.Models.QuestionLogEntry> QuestionLogEntry { get; set; }
        public DbSet<SupportBot.Services.QnAMakerService> QnAMakerService { get; set; }
        public DbSet<SupportBot.Models.UserFeedback> UserFeedback { get; set; }
    }
}
