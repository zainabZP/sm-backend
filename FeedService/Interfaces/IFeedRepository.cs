using FeedService.Entities;

namespace FeedService.Interfaces {
    public interface IFeedRepository {
        Task<List<DateTime>> GetExistingPostCreatedTimes(int userId, List<DateTime> createdTimes);
        Task SaveFeedItems(List<FeedItem> feedItems);
        Task<List<FeedItem>> GetFeedItemsByUserId(int userId);
    }
}
