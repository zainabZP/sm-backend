using CommentService.DTOs;

namespace CommentService.Interfaces {
    public interface ICommentService {
        Task<CommentResponseDto> AddComment(int userId, CreateCommentDto dto);
        Task<List<CommentResponseDto>> GetCommentsByPost(int postId);
        Task<List<CommentedPostDto>> GetPostsCommentedByUser(int userId);
        Task<bool> DeleteComment(int commentId, int userId);
    }
}
