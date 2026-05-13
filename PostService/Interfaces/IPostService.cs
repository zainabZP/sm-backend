using PostService.DTOs;

namespace PostService.Interfaces {
    public interface IPostService {
        Task<PostResponseDto?> GetPostById(int postId, int? currentUserId = null);
        Task<List<PostResponseDto>> GetPostsByUserId(int userId, int? currentUserId = null);
        Task<List<PostResponseDto>> GetPublicPosts();
        Task<List<PostResponseDto>> GetPostsByHashtag(string hashtag);
        Task<List<PostResponseDto>> SearchPosts(string keyword);
        Task<List<PostResponseDto>> GetTrendingPosts();
        Task<List<PostResponseDto>> GetFeedPosts(List<int> followingIds);
        Task<PostResponseDto> CreatePost(int userId, CreatePostDto dto);
        Task<PostResponseDto?> UpdatePost(int postId, int userId, UpdatePostDto dto);
        Task<bool> DeletePost(int postId, int userId);
        Task IncrementLikeCount(int postId);
        Task DecrementLikeCount(int postId);
        Task IncrementCommentCount(int postId);
        Task DecrementCommentCount(int postId);
        Task IncrementShareCount(int postId);
    }
}