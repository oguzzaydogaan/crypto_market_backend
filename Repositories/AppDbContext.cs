using Microsoft.EntityFrameworkCore;
using Repositories.Entities;

namespace Repositories
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<Coin> Coins { get; set; }
    }
}
