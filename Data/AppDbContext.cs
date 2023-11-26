using Microsoft.EntityFrameworkCore;
using RedisCaching.Models;

namespace RedisCaching.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Driver> Drivers { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
