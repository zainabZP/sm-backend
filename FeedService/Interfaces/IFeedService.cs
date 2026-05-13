using FeedService.DTOs;

namespace FeedService.Interfaces {
    public interface IFeedService {
        Task<List<PostResponseDto>> GetFollowingFeed(int userId);
        Task<List<PostResponseDto>> GetExploreFeed();
    }
}
