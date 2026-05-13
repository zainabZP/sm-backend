using FollowService.Entities;

namespace FollowService.Interfaces {
    public interface IFollowRepository {
        Task<Follow?> GetFollow(int followerId, int followeeId);
        Task<Follow?> GetFollowById(int followId);
        Task<List<Follow>> GetFollowers(int userId);
        Task<List<Follow>> GetFollowing(int userId);
        Task<List<Follow>> GetPendingRequests(int userId);
        Task<bool> IsFollowing(int followerId, int followeeId);
        Task<Follow> CreateFollow(Follow follow);
        Task<bool> UpdateFollowStatus(int followId, string status);
        Task<bool> DeleteFollow(int followerId, int followeeId);
        Task<int> GetFollowerCount(int userId);
        Task<int> GetFollowingCount(int userId);
    }
}