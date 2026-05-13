using LikeService.Entities;

namespace LikeService.Interfaces {
    public interface ILikeRepository {
        Task<Like?> GetLike(int userId, int targetId, string targetType);
        Task<List<Like>> GetLikesByTarget(int targetId, string targetType);
        Task<List<Like>> GetLikesByUser(int userId);
        Task<int> GetLikeCount(int targetId, string targetType);
        Task<bool> HasUserLiked(int userId, int targetId, string targetType);
        Task<Like> AddLike(Like like);
        Task<bool> RemoveLike(int userId, int targetId, string targetType);
    }
}