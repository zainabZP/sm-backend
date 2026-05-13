using LikeService.DTOs;
using LikeService.Entities;
using LikeService.Interfaces;

namespace LikeService.Services {
    public class LikeService : ILikeService {
        private readonly ILikeRepository _likeRepository;
        private readonly PostServiceClient _postServiceClient;

        public LikeService(ILikeRepository likeRepository, PostServiceClient postServiceClient) {
            _likeRepository = likeRepository;
            _postServiceClient = postServiceClient;
        }

        public async Task<bool> ToggleLike(int userId, ToggleLikeDto dto) {
            bool hasLiked = await _likeRepository.HasUserLiked(userId, dto.TargetId, dto.TargetType);

            if (hasLiked) {
                await _likeRepository.RemoveLike(userId, dto.TargetId, dto.TargetType);
                if (string.Equals(dto.TargetType, "POST", StringComparison.OrdinalIgnoreCase)) {
                    await _postServiceClient.DecrementLikeCount(dto.TargetId);
                }
                return false;
            } else {
                Like like = new Like {
                    UserId = userId,
                    TargetId = dto.TargetId,
                    TargetType = dto.TargetType
                };
                await _likeRepository.AddLike(like);
                if (string.Equals(dto.TargetType, "POST", StringComparison.OrdinalIgnoreCase)) {
                    await _postServiceClient.IncrementLikeCount(dto.TargetId);
                }
                return true;
            }
        }

        public async Task<List<LikeResponseDto>> GetLikesByTarget(int targetId, string targetType) {
            List<Like> likes = await _likeRepository.GetLikesByTarget(targetId, targetType);
            return likes.Select(MapToDto).ToList();
        }

        public async Task<List<LikeResponseDto>> GetLikesByUser(int userId) {
            List<Like> likes = await _likeRepository.GetLikesByUser(userId);
            return likes.Select(MapToDto).ToList();
        }

        public async Task<int> GetLikeCount(int targetId, string targetType) {
            return await _likeRepository.GetLikeCount(targetId, targetType);
        }

        public async Task<bool> HasUserLiked(int userId, int targetId, string targetType) {
            return await _likeRepository.HasUserLiked(userId, targetId, targetType);
        }

        private LikeResponseDto MapToDto(Like like) {
            return new LikeResponseDto {
                LikeId = like.LikeId,
                UserId = like.UserId,
                TargetId = like.TargetId,
                TargetType = like.TargetType,
                CreatedAt = like.CreatedAt
            };
        }
    }
}