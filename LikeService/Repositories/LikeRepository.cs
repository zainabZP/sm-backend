using Microsoft.EntityFrameworkCore;
using LikeService.Context;
using LikeService.Entities;
using LikeService.Interfaces;

namespace LikeService.Repositories {
    public class LikeRepository : ILikeRepository {
        private readonly AppDbContext _context;

        public LikeRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<Like?> GetLike(int userId, int targetId, string targetType) {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId &&
                                         l.TargetId == targetId &&
                                         l.TargetType == targetType);
        }

        public async Task<List<Like>> GetLikesByTarget(int targetId, string targetType) {
            return await _context.Likes
                .Where(l => l.TargetId == targetId && l.TargetType == targetType)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Like>> GetLikesByUser(int userId) {
            return await _context.Likes
                .Where(l => l.UserId == userId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> GetLikeCount(int targetId, string targetType) {
            return await _context.Likes
                .CountAsync(l => l.TargetId == targetId && l.TargetType == targetType);
        }

        public async Task<bool> HasUserLiked(int userId, int targetId, string targetType) {
            return await _context.Likes
                .AnyAsync(l => l.UserId == userId &&
                               l.TargetId == targetId &&
                               l.TargetType == targetType);
        }

        public async Task<Like> AddLike(Like like) {
            await _context.Likes.AddAsync(like);
            await _context.SaveChangesAsync();
            return like;
        }

        public async Task<bool> RemoveLike(int userId, int targetId, string targetType) {
            Like? like = await _context.Likes
                .FirstOrDefaultAsync(l => l.UserId == userId &&
                                         l.TargetId == targetId &&
                                         l.TargetType == targetType);
            if (like == null) return false;
            _context.Likes.Remove(like);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}