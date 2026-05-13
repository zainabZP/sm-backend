using Microsoft.EntityFrameworkCore;
using CommentService.Context;
using CommentService.Entities;
using CommentService.Interfaces;

namespace CommentService.Repositories {
    public class CommentRepository : ICommentRepository {
        private readonly AppDbContext _context;

        public CommentRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<Comment> AddComment(Comment comment) {
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<Comment?> GetCommentById(int commentId) {
            return await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        // Returns all non-deleted comments for a post (flat list; tree built in service)
        public async Task<List<Comment>> GetCommentsByPost(int postId) {
            return await _context.Comments
                .Where(c => c.PostId == postId && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }

        // Returns all non-deleted comments made by a user
        public async Task<List<Comment>> GetCommentsByUser(int userId) {
            return await _context.Comments
                .Where(c => c.UserId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        // Soft-deletes a comment and all its descendants recursively
        public async Task<bool> SoftDeleteComment(int commentId, int userId) {
            Comment? comment = await _context.Comments
                .FirstOrDefaultAsync(c => c.CommentId == commentId && c.UserId == userId && !c.IsDeleted);

            if (comment == null) return false;

            // Recursively soft-delete all descendants
            await SoftDeleteDescendants(commentId);

            // Soft-delete the target comment itself
            comment.IsDeleted = true;
            comment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Recursively marks all child comments as deleted
        private async Task SoftDeleteDescendants(int parentCommentId) {
            List<Comment> children = await _context.Comments
                .Where(c => c.ParentCommentId == parentCommentId && !c.IsDeleted)
                .ToListAsync();

            foreach (Comment child in children) {
                child.IsDeleted = true;
                child.UpdatedAt = DateTime.UtcNow;
                await SoftDeleteDescendants(child.CommentId);
            }
        }
    }
}
