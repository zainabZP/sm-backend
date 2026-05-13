using Microsoft.EntityFrameworkCore;
using FeedService.Entities;

namespace FeedService.Context {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<FeedItem> FeedItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            // Unique pair of FeedOwnerId and PostId to ensure a post only appears once in a user's feed
            modelBuilder.Entity<FeedItem>()
                .HasIndex(f => new { f.FeedOwnerId, f.PostId })
                .IsUnique();
                
            modelBuilder.Entity<FeedItem>()
                .HasIndex(f => f.FeedOwnerId);
        }
    }
}
