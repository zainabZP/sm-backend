using AuthService.DTOs;

namespace AuthService.Interfaces {
    public interface IAuthService {
        Task<AuthResponseDto?> Register(RegisterDto dto);
        Task<AuthResponseDto?> Login(LoginDto dto);
        Task<bool> Logout(string token);
        Task<UserResponseDto?> GetUserById(int userId);
        Task<List<UserResponseDto>> SearchUsers(string keyword);
        Task<UserResponseDto?> UpdateProfile(int userId, UpdateProfileDto dto);
        Task<bool> ChangePassword(int userId, ChangePasswordDto dto);
        Task<bool> TogglePrivacy(int userId);
        Task<bool> DeactivateAccount(int userId);
        Task<bool> IsUserPrivate(int userId);
        Task<List<UserResponseDto>> GetUsersByIds(List<int> userIds);
        Task IncrementFollowerCount(int userId);
        Task DecrementFollowerCount(int userId);
        Task IncrementFollowingCount(int userId);
        Task DecrementFollowingCount(int userId);
        Task SetFollowCounts(int userId, int followerCount, int followingCount);
        Task<List<UserResponseDto>> GetRecommendedUsers(int currentUserId);
    }
}