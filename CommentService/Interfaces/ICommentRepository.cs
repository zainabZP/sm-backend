using CommentService.Entities;

namespace CommentService.Interfaces {
    public interface ICommentRepository {
        Task<Comment> AddComment(Comment comment);
        Task<Comment?> GetCommentById(int commentId);
        Task<List<Comment>> GetCommentsByPost(int postId);
        Task<List<Comment>> GetCommentsByUser(int userId);
        Task<bool> SoftDeleteComment(int commentId, int userId);
    }
}
