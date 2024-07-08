using Aspbackend.Model;
using Microsoft.EntityFrameworkCore;

namespace Aspbackend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        //User is the model
        public DbSet<User> Users { get; set; } = default!;

        //passwordreset model
        public DbSet<PasswordReset> PasswordResets { get; set; }

        public DbSet<PasswordResetToken> PasswordResetTokens { get; set; }

        //bank model
        public DbSet<BankLink> BankLinks { get; set; }

        //card model
        public DbSet<CardLink> CardLinks { get; set; }

        //card model
        public DbSet<VerificationCode> VerificationCodes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>().ToTable("Users"); // Adjust the table name to match your database
            
        }
        public DbSet<Aspbackend.Model.CardLink> CardLink { get; set; } = default!;
    }
}