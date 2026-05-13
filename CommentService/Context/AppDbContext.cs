using Microsoft.EntityFrameworkCore;
using CommentService.Entities;

namespace CommentService.Context {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Comment>(entity => {
                // Self-referencing relationship: a comment can have a parent comment
                entity.HasOne(c => c.ParentComment)
                      .WithMany(c => c.Replies)
                      .HasForeignKey(c => c.ParentCommentId)
                      .OnDelete(DeleteBehavior.Restrict); // Soft-delete strategy, no DB cascade

                entity.HasIndex(c => c.PostId);
                entity.HasIndex(c => c.UserId);
                entity.HasIndex(c => c.ParentCommentId);
            });
        }
    }
}
