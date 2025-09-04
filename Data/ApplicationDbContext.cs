using Microsoft.EntityFrameworkCore;
using QuizBoard.Models;

namespace QuizBoard.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<UserInfo> Users { get; set; }
        public DbSet<Question> QuestionTable { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Question entity to map to QuestionTable
            modelBuilder.Entity<Question>()
                .ToTable("QuestionTable");
        }
    }
}