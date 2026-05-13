using FeedService.Context;
using FeedService.Entities;
using FeedService.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FeedService.Repositories {
    public class FeedRepository : IFeedRepository {
        private readonly AppDbContext _context;

        public FeedRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<List<DateTime>> GetExistingPostCreatedTimes(int userId, List<DateTime> createdTimes) {
            return await _context.FeedItems
                .Where(f => f.FeedOwnerId == userId && createdTimes.Contains(f.CreatedAt))
                .Select(f => f.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveFeedItems(List<FeedItem> feedItems) {
            await _context.FeedItems.AddRangeAsync(feedItems);
            await _context.SaveChangesAsync();
        }

        public async Task<List<FeedItem>> GetFeedItemsByUserId(int userId) {
            return await _context.FeedItems
                .Where(f => f.FeedOwnerId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .ToListAsync();
        }
    }
}
