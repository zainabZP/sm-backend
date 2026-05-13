using PostService.Entities;

namespace PostService.Interfaces {
    public interface IPostRepository {
        Task<Post?> GetPostById(int postId);
        Task<List<Post>> GetPostsByUserId(int userId);
        Task<List<Post>> GetPublicPosts();
        Task<List<Post>> GetPostsByHashtag(string hashtag);
        Task<List<Post>> SearchPosts(string keyword);
        Task<List<Post>> GetTrendingPosts();
        Task<List<Post>> GetFeedPosts(List<int> followingIds);
        Task<Post> CreatePost(Post post);
        Task<Post?> UpdatePost(int postId, string content, string visibility, string? hashtags);
        Task<bool> DeletePost(int postId);
        Task IncrementLikeCount(int postId);
        Task DecrementLikeCount(int postId);
        Task IncrementCommentCount(int postId);
        Task DecrementCommentCount(int postId);
        Task IncrementShareCount(int postId);
    }
}