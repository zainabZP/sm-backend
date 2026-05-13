using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.DTOs;
using AuthService.Entities;
using AuthService.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Services {
    public class AuthService : IAuthService {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;

        public AuthService(IUserRepository userRepository, IConfiguration configuration) {
            _userRepository = userRepository;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto?> Register(RegisterDto dto) {
            bool emailExists = await _userRepository.UserExistsByEmail(dto.Email);
            if (emailExists) return null;

            bool userNameExists = await _userRepository.UserExistsByUserName(dto.UserName);
            if (userNameExists) return null;

            User user = new User {
                UserName = dto.UserName,
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            await _userRepository.AddUser(user);
            return GenerateToken(user);
        }

        public async Task<AuthResponseDto?> Login(LoginDto dto) {
            User? user = await _userRepository.GetUserByEmail(dto.Email);
            if (user == null) return null;

            bool passwordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid) return null;

            return GenerateToken(user);
        }

        public async Task<bool> Logout(string token) {
            bool isAlreadyBlacklisted = await _userRepository.IsTokenBlacklisted(token);
            if (isAlreadyBlacklisted) return false;

            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken? jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
            DateTime expiresAt =jwtToken != null ? jwtToken.ValidTo : DateTime.UtcNow.AddDays(7);  // if token is not null then its own expire time will be used as store it for 7 days as blacklisted in db then it will be automatically removed. its done in case parsing is failed from string to jwtToken .

            await _userRepository.BlacklistToken(token, expiresAt);
            return true;
        }

        public async Task<UserResponseDto?> GetUserById(int userId) {
            User? user = await _userRepository.GetUserById(userId);
            if (user == null) return null;
            return MapToDto(user);
        }

        public async Task<List<UserResponseDto>> SearchUsers(string keyword) {
            List<User> users = await _userRepository.SearchUsers(keyword);
            return users.Select(u=>MapToDto(u)).ToList();
        }

        public async Task<UserResponseDto?> UpdateProfile(int userId, UpdateProfileDto dto) {
            User? user = await _userRepository.GetUserById(userId);
            if (user == null) return null;

            await _userRepository.UpdateProfile(userId, dto.FullName, dto.Bio, dto.AvatarUrl);

            User? updated = await _userRepository.GetUserById(userId);
            if (updated == null) return null;
            return MapToDto(updated);
        }

        public async Task<bool> ChangePassword(int userId, ChangePasswordDto dto) {
            User? user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            bool currentPasswordValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
            if (!currentPasswordValid) return false;

            string newPasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _userRepository.ChangePassword(userId, newPasswordHash);
            return true;
        }

        public async Task<bool> TogglePrivacy(int userId) {
            User? user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            await _userRepository.TogglePrivacy(userId);
            return true;
        }

        public async Task<bool> DeactivateAccount(int userId) {
            User? user = await _userRepository.GetUserById(userId);
            if (user == null) return false;

            await _userRepository.DeactivateAccount(userId);
            return true;
        }

        public async Task<bool> IsUserPrivate(int userId) {
            return await _userRepository.IsUserPrivate(userId);
        }

        // used by follow service to get followers
        public async Task<List<UserResponseDto>> GetUsersByIds(List<int> userIds) {
            List<User> users = await _userRepository.GetUsersByIds(userIds);
            return users.Select(MapToDto).ToList();
        }

        public async Task IncrementFollowerCount(int userId) {
            await _userRepository.IncrementFollowerCount(userId);
        }

        public async Task DecrementFollowerCount(int userId) {
            await _userRepository.DecrementFollowerCount(userId);
        }

        public async Task IncrementFollowingCount(int userId) {
            await _userRepository.IncrementFollowingCount(userId);
        }

        public async Task DecrementFollowingCount(int userId) {
            await _userRepository.DecrementFollowingCount(userId);
        }

        public async Task<List<UserResponseDto>> GetRecommendedUsers(int currentUserId) {
            List<User> users = await _userRepository.GetPublicUsers(10);
            return users
                .Where(u => u.UserId != currentUserId)
                .Select(MapToDto)
                .ToList();
        }

        public async Task SetFollowCounts(int userId, int followerCount, int followingCount) {
            await _userRepository.SetFollowCounts(userId, followerCount, followingCount);
        }

        private AuthResponseDto GenerateToken(User user) {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            byte[] key = Encoding.UTF8.GetBytes(
                _configuration["Jwt:Secret"] ?? throw new Exception("JWT Secret not configured")
            );

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponseDto {
                Token = tokenHandler.WriteToken(token),
                UserName = user.UserName,
                Email = user.Email,
                UserId = user.UserId
            };
        }

        private UserResponseDto MapToDto(User user) {
            return new UserResponseDto {
                UserId = user.UserId,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                IsPrivate = user.IsPrivate,
                FollowerCount = user.FollowerCount,
                FollowingCount = user.FollowingCount,
                PostCount = user.PostCount,
                CreatedAt = user.CreatedAt
            };
        }
    }
}