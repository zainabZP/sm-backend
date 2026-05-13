using Microsoft.EntityFrameworkCore;
using PostService.Entities;

namespace PostService.Context {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Post> Posts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Post>()
                .HasIndex(p => new { p.UserId, p.CreatedAt });

            modelBuilder.Entity<Post>()
                .HasIndex(p => p.Hashtags);
        }
    }
}