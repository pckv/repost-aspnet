using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }
        
        public DbSet<Resub> Resubs { get; set; }
    }
}