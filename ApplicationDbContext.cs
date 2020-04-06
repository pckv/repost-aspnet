using Microsoft.EntityFrameworkCore;
using RepostAspNet.Models;

namespace RepostAspNet
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Resub> Resubs { get; set; }
        public DbSet<Post> Posts { get; set; }
    }
}