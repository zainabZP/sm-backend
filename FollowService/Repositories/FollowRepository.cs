using Microsoft.EntityFrameworkCore;
using FollowService.Context;
using FollowService.Entities;
using FollowService.Interfaces;

namespace FollowService.Repositories {
    public class FollowRepository : IFollowRepository {
        private readonly AppDbContext _context;

        public FollowRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<Follow?> GetFollow(int followerId, int followeeId) {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && 
                                         f.FolloweeId == followeeId);
        }

        public async Task<Follow?> GetFollowById(int followId) {
            return await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowId == followId);
        }

        public async Task<List<Follow>> GetFollowers(int userId) {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId && f.Status == "ACCEPTED")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Follow>> GetFollowing(int userId) {
            return await _context.Follows
                .Where(f => f.FollowerId == userId && f.Status == "ACCEPTED")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Follow>> GetPendingRequests(int userId) {
            return await _context.Follows
                .Where(f => f.FolloweeId == userId && f.Status == "PENDING")
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsFollowing(int followerId, int followeeId) {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && 
                               f.FolloweeId == followeeId && 
                               f.Status == "ACCEPTED");
        }

        public async Task<Follow> CreateFollow(Follow follow) {
            await _context.Follows.AddAsync(follow);
            await _context.SaveChangesAsync();
            return follow;
        }

        public async Task<bool> UpdateFollowStatus(int followId, string status) {
            Follow? follow = await _context.Follows.FindAsync(followId);
            if (follow == null) return false;
            follow.Status = status;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteFollow(int followerId, int followeeId) {
            Follow? follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && 
                                         f.FolloweeId == followeeId);
            if (follow == null) return false;
            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetFollowerCount(int userId) {
            return await _context.Follows.CountAsync(f => f.FolloweeId == userId && f.Status == "ACCEPTED");
        }

        public async Task<int> GetFollowingCount(int userId) {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId && f.Status == "ACCEPTED");
        }
    }
}