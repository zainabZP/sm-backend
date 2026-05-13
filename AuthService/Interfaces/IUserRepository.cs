using AuthService.Entities;

namespace AuthService.Interfaces {
    public interface IUserRepository {
        Task<bool> UserExistsByEmail(string email);
        Task<bool> UserExistsByUserName(string userName);
        Task<User?> GetUserByEmail(string email);
        Task<User?> GetUserById(int userId);
        Task AddUser(User user);
        Task BlacklistToken(string token, DateTime expiresAt);
        Task<bool> IsTokenBlacklisted(string token);
        Task<List<User>> SearchUsers(string keyword);
        Task UpdateProfile(int userId, string? fullName, string? bio, string? avatarUrl);
        Task ChangePassword(int userId, string newPasswordHash);
        Task TogglePrivacy(int userId);
        Task DeactivateAccount(int userId);
         Task<bool> IsUserPrivate(int userId);
        Task<List<User>> GetUsersByIds(List<int> userIds);
        Task IncrementFollowerCount(int userId);
        Task DecrementFollowerCount(int userId);
        Task IncrementFollowingCount(int userId);
        Task DecrementFollowingCount(int userId);
        Task<List<User>> GetPublicUsers(int limit);
        Task SetFollowCounts(int userId, int followerCount, int followingCount);
    }
}