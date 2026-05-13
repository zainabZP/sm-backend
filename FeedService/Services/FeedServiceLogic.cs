using FeedService.Clients;
using FeedService.DTOs;
using FeedService.Interfaces;
using FeedService.Entities;


namespace FeedService.Services {
    public class FeedServiceLogic : IFeedService {
        private readonly FollowServiceClient _followClient;
        private readonly PostServiceClient _postClient;
        private readonly IFeedRepository _feedRepository;

        public FeedServiceLogic(FollowServiceClient followClient, PostServiceClient postClient, IFeedRepository feedRepository) {
            _followClient = followClient;
            _postClient = postClient;
            _feedRepository = feedRepository;
        }

        public async Task<List<PostResponseDto>> GetFollowingFeed(int userId) {
            // 1. Try to get pre-calculated feed from DB
            var feedItems = await _feedRepository.GetFeedItemsByUserId(userId);
            
            if (feedItems.Any()) {
                // If we have pre-calculated items, fetch the actual post content
                var postIds = feedItems.OrderByDescending(f => f.CreatedAt).Take(50).Select(f => f.PostId).ToList();
                var posts = await _postClient.GetPostsByIds(postIds);
                return posts.OrderByDescending(p => p.CreatedAt).ToList();
            }

            // 2. Fallback: If no pre-calculated feed, use the old synchronous method
            var followingIds = await _followClient.GetFollowingUserIds(userId);
            followingIds.Add(userId); 
            
            var fallbackPosts = await _postClient.GetFeedPosts(followingIds);

            // Optional: Save these to DB so they are pre-calculated for next time
            var fetchedTimes = fallbackPosts.Select(p => p.CreatedAt).ToList();
            var existingTimes = await _feedRepository.GetExistingPostCreatedTimes(userId, fetchedTimes);
            var newFeedItems = fallbackPosts
                .Where(p => !existingTimes.Contains(p.CreatedAt))
                .Select(p => new FeedItem {
                    FeedOwnerId = userId,
                    PostId = p.PostId,
                    PostAuthorId = p.UserId,
                    CreatedAt = p.CreatedAt
                }).ToList();

            if (newFeedItems.Any()) {
                await _feedRepository.SaveFeedItems(newFeedItems);
            }

            return fallbackPosts.OrderByDescending(p => p.CreatedAt).ToList();
        }

        public async Task<List<PostResponseDto>> GetExploreFeed() {
            var posts = await _postClient.GetPublicPosts();
            return posts.OrderByDescending(p => p.CreatedAt).ToList();
        }
    }
}
