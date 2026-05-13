using LikeService.DTOs;

namespace LikeService.Interfaces {
    public interface ILikeService {
        Task<bool> ToggleLike(int userId, ToggleLikeDto dto);
        Task<List<LikeResponseDto>> GetLikesByTarget(int targetId, string targetType);
        Task<List<LikeResponseDto>> GetLikesByUser(int userId);
        Task<int> GetLikeCount(int targetId, string targetType);
        Task<bool> HasUserLiked(int userId, int targetId, string targetType);
    }
}