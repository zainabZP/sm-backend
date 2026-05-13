using FollowService.DTOs;

namespace FollowService.Interfaces {
    public interface IFollowService {
        Task<FollowResponseDto?> FollowUser(int followerId, int followeeId);
        Task<bool> UnfollowUser(int followerId, int followeeId);
        Task<bool> AcceptFollowRequest(int followId, int userId);
        Task<bool> RejectFollowRequest(int followId, int userId);
        Task<List<FollowResponseDto>> GetFollowers(int userId);
        Task<List<FollowResponseDto>> GetFollowing(int userId);
        Task<List<FollowResponseDto>> GetPendingRequests(int userId);
        Task<FollowResponseDto?> IsFollowing(int followerId, int followeeId);
        Task<bool> SyncUserCounts(int userId);
        Task<int> GetFollowerCount(int userId);
        Task<int> GetFollowingCount(int userId);
    }
}