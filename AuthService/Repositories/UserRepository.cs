using Microsoft.EntityFrameworkCore;
using AuthService.Context;
using AuthService.Entities;
using AuthService.Interfaces;

namespace AuthService.Repositories {
    public class UserRepository : IUserRepository {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context) {
            _context = context;
        }


        public async Task<bool> UserExistsByEmail(string email) {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> UserExistsByUserName(string userName) {
            return await _context.Users.AnyAsync(u => u.UserName == userName);
        }

        public async Task<User?> GetUserByEmail(string email) {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User?> GetUserById(int userId) {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        }

        public async Task AddUser(User user) {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }
        public async Task BlacklistToken(string token, DateTime expiresAt) {
            BlacklistedToken blacklistedToken = new BlacklistedToken {
                Token = token,
                ExpiresAt = expiresAt
            };
            await _context.BlacklistedTokens.AddAsync(blacklistedToken);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsTokenBlacklisted(string token) {
            return await _context.BlacklistedTokens.AnyAsync(b => b.Token == token);
        }

        public async Task<List<User>> SearchUsers(string keyword) {
            return await _context.Users
                .Where(u => u.IsActive &&
                       (u.UserName.Contains(keyword) ||
                        u.FullName.Contains(keyword)))
                .ToListAsync();
        }

        public async Task UpdateProfile(int userId, string? fullName, string? bio, string? avatarUrl) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FullName, x => fullName ?? x.FullName)
                    .SetProperty(x => x.Bio, x => bio ?? x.Bio)
                    .SetProperty(x => x.AvatarUrl, x => avatarUrl ?? x.AvatarUrl));
        }

        public async Task ChangePassword(int userId, string newPasswordHash) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.PasswordHash, newPasswordHash));
        }

        public async Task TogglePrivacy(int userId) {
            User? user = await _context.Users.FindAsync(userId);
            if (user == null) return;
            user.IsPrivate = !user.IsPrivate;
            await _context.SaveChangesAsync();
        }

        public async Task DeactivateAccount(int userId) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.IsActive, false));
        }

        public async Task<bool> IsUserPrivate(int userId) {
            User? user = await _context.Users.FindAsync(userId);
            return user?.IsPrivate ?? false;
        }

        // used by follow service to get followers
        public async Task<List<User>> GetUsersByIds(List<int> userIds) {
            return await _context.Users
                .Where(u => userIds.Contains(u.UserId))
                .ToListAsync();
        }

        public async Task IncrementFollowerCount(int userId) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FollowerCount, x => x.FollowerCount + 1));
        }

        public async Task DecrementFollowerCount(int userId) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FollowerCount, x => x.FollowerCount - 1));
        }

        public async Task IncrementFollowingCount(int userId) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FollowingCount, x => x.FollowingCount + 1));
        }

        public async Task DecrementFollowingCount(int userId) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FollowingCount, x => x.FollowingCount - 1));
        }

        public async Task<List<User>> GetPublicUsers(int limit) {
            return await _context.Users
                .Where(u => u.IsActive && !u.IsPrivate)
                .Take(limit)
                .ToListAsync();
        }

        public async Task SetFollowCounts(int userId, int followerCount, int followingCount) {
            await _context.Users
                .Where(u => u.UserId == userId)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(x => x.FollowerCount, followerCount)
                    .SetProperty(x => x.FollowingCount, followingCount));
        }
    }
}