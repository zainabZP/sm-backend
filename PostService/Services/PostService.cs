using PostService.Clients;
using PostService.DTOs;
using PostService.Entities;
using PostService.Interfaces;

namespace PostService.Services {
    public class PostService : IPostService {
        private readonly IPostRepository _postRepository;
        private readonly FollowServiceClient _followClient;
        private readonly AuthServiceClient _authClient;
        private readonly IRabbitMQPublisher _rabbitMQPublisher;

        public PostService(IPostRepository postRepository, FollowServiceClient followClient, AuthServiceClient authClient, IRabbitMQPublisher rabbitMQPublisher) {
            _postRepository = postRepository;
            _followClient = followClient;
            _authClient = authClient;
            _rabbitMQPublisher = rabbitMQPublisher;
        }

        public async Task<PostResponseDto?> GetPostById(int postId, int? currentUserId = null) {
            Post? post = await _postRepository.GetPostById(postId);
            if (post == null) return null;

            if (post.UserId != currentUserId) {
                bool isUserPrivate = await _authClient.IsUserPrivate(post.UserId);
                bool isFollowing = false;
                if (currentUserId != null) {
                    isFollowing = await _followClient.IsFollowing(currentUserId.Value, post.UserId);
                }

                // If user is private and not following, hide the post
                if (isUserPrivate && !isFollowing) return null;

                // If post is not public and not following, hide the post
                if (post.Visibility != "PUBLIC" && !isFollowing) return null;
            }

            var dto = MapToDto(post);
            await EnrichPosts(new List<PostResponseDto> { dto });
            return dto;
        }

        public async Task<List<PostResponseDto>> GetPostsByUserId(int userId, int? currentUserId = null) {
            List<Post> posts = await _postRepository.GetPostsByUserId(userId);
            
            if (userId != currentUserId) {
                bool isFollowing = false;
                if (currentUserId != null) {
                    isFollowing = await _followClient.IsFollowing(currentUserId.Value, userId);
                }

                bool isUserPrivate = await _authClient.IsUserPrivate(userId);
                if (isUserPrivate && !isFollowing) {
                    return new List<PostResponseDto>();
                }

                posts = posts.Where(p => p.Visibility == "PUBLIC" || isFollowing).ToList();
            }

            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos;
        }

        public async Task<List<PostResponseDto>> GetPublicPosts() {
            List<Post> posts = await _postRepository.GetPublicPosts();
            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos.Where(p => !p.IsUserPrivate).ToList();
        }

        public async Task<List<PostResponseDto>> GetPostsByHashtag(string hashtag) {
            List<Post> posts = await _postRepository.GetPostsByHashtag(hashtag);
            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos.Where(p => !p.IsUserPrivate).ToList();
        }

        public async Task<List<PostResponseDto>> SearchPosts(string keyword) {
            List<Post> posts = await _postRepository.SearchPosts(keyword);
            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos.Where(p => !p.IsUserPrivate).ToList();
        }

        public async Task<List<PostResponseDto>> GetTrendingPosts() {
            List<Post> posts = await _postRepository.GetTrendingPosts();
            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos.Where(p => !p.IsUserPrivate).ToList();
        }

        public async Task<List<PostResponseDto>> GetFeedPosts(List<int> followingIds) {
            List<Post> posts = await _postRepository.GetFeedPosts(followingIds);
            var dtos = posts.Select(MapToDto).ToList();
            await EnrichPosts(dtos);
            return dtos;
        }

        public async Task<PostResponseDto> CreatePost(int userId, CreatePostDto dto) {
            Post post = new Post {
                UserId = userId,
                Content = dto.Content,
                MediaUrl = dto.MediaUrl,
                MediaType = dto.MediaType,
                Visibility = dto.Visibility,
                Hashtags = dto.Hashtags
            };

            Post created = await _postRepository.CreatePost(post);
            var response = MapToDto(created);
            await EnrichPosts(new List<PostResponseDto> { response });

            // Publish event to RabbitMQ
            _ = _rabbitMQPublisher.PublishPostCreated(new global::PostService.Events.PostCreatedEvent {
                PostId = response.PostId,
                UserId = response.UserId,
                CreatedAt = response.CreatedAt
            });

            return response;
        }

        public async Task<PostResponseDto?> UpdatePost(int postId, int userId, UpdatePostDto dto) {
            Post? existing = await _postRepository.GetPostById(postId);
            if (existing == null) return null;
            if (existing.UserId != userId) return null;

            Post? updated = await _postRepository.UpdatePost(
                postId,
                dto.Content,
                dto.Visibility,
                dto.Hashtags
            );

            if (updated == null) return null;
            return MapToDto(updated);
        }

        public async Task<bool> DeletePost(int postId, int userId) {
            Post? existing = await _postRepository.GetPostById(postId);
            if (existing == null) return false;
            if (existing.UserId != userId) return false;

            return await _postRepository.DeletePost(postId);
        }

        public async Task IncrementLikeCount(int postId) {
            await _postRepository.IncrementLikeCount(postId);
        }

        public async Task DecrementLikeCount(int postId) {
            await _postRepository.DecrementLikeCount(postId);
        }

        public async Task IncrementCommentCount(int postId) {
            await _postRepository.IncrementCommentCount(postId);
        }

        public async Task DecrementCommentCount(int postId) {
            await _postRepository.DecrementCommentCount(postId);
        }

        public async Task IncrementShareCount(int postId) {
            await _postRepository.IncrementShareCount(postId);
        }

        private async Task EnrichPosts(List<PostResponseDto> posts) {
            if (posts == null || !posts.Any()) return;

            var userIds = posts.Select(p => p.UserId).Distinct().ToList();
            var users = await _authClient.GetUsersByIds(userIds);
            var userDict = users.ToDictionary(u => u.UserId);

            foreach (var post in posts) {
                if (userDict.TryGetValue(post.UserId, out var user)) {
                    post.UserName = user.UserName;
                    post.FullName = user.FullName;
                    post.AvatarUrl = user.AvatarUrl;
                    post.IsUserPrivate = user.IsPrivate;
                }
            }
        }

        private PostResponseDto MapToDto(Post post) {
            return new PostResponseDto {
                PostId = post.PostId,
                UserId = post.UserId,
                Content = post.Content,
                MediaUrl = post.MediaUrl,
                MediaType = post.MediaType,
                Visibility = post.Visibility,
                Hashtags = post.Hashtags,
                LikeCount = post.LikeCount,
                CommentCount = post.CommentCount,
                ShareCount = post.ShareCount,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt
            };
        }
    }
}