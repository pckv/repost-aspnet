using System.Text.RegularExpressions;
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
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentVote> CommentsVotes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Assign composite keys to vote tables
            builder.Entity<PostVote>().HasKey(p => new {p.UserId, p.PostId});
            builder.Entity<CommentVote>().HasKey(c => new {c.UserId, c.CommentId});

            // Convert all PascalCase names to snake_case
            foreach (var entity in builder.Model.GetEntityTypes())
            {
                entity.SetTableName(ToSnakeCase(entity.GetTableName()));

                foreach (var property in entity.GetProperties())
                {
                    property.SetColumnName(ToSnakeCase(property.GetColumnName()));
                }

                foreach (var key in entity.GetKeys())
                {
                    key.SetName(ToSnakeCase(key.GetName()));
                }

                foreach (var key in entity.GetForeignKeys())
                {
                    key.SetConstraintName(ToSnakeCase(key.GetConstraintName()));
                }

                foreach (var index in entity.GetIndexes())
                {
                    index.SetName(ToSnakeCase(index.GetName()));
                }
            }
        }

        private string ToSnakeCase(string text)
        {
            return string.IsNullOrEmpty(text)
                ? text
                : Regex.Replace(text, @"([a-z0-9])([A-Z])", "$1_$2").ToLower();
        }
    }
}