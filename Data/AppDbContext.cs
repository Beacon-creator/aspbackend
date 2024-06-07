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
    }
}
