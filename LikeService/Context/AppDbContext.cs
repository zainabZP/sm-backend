using Microsoft.EntityFrameworkCore;
using LikeService.Entities;

namespace LikeService.Context {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Like> Likes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Like>()
                .HasIndex(l => new { l.UserId, l.TargetId, l.TargetType })
                .IsUnique();
        }
    }
}