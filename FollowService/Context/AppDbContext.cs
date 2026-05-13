using Microsoft.EntityFrameworkCore;
using FollowService.Entities;

namespace FollowService.Context {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Follow> Follows { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Follow>()
                .HasIndex(f => new { f.FollowerId, f.FolloweeId })
                .IsUnique();

            modelBuilder.Entity<Follow>()
                .HasIndex(f => f.FolloweeId);
        }
    }
}