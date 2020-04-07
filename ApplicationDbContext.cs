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
        public DbSet<PostVote> PostsVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<PostVote>().HasKey(p => new {p.UserId, p.PostId});
        }
    }
}