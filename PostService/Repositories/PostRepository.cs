using Microsoft.EntityFrameworkCore;
using PostService.Context;
using PostService.Entities;
using PostService.Interfaces;

namespace PostService.Repositories {
    public class PostRepository : IPostRepository {
        private readonly AppDbContext _context;

        public PostRepository(AppDbContext context) {
            _context = context;
        }

        public async Task<Post?> GetPostById(int postId) {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);
        }

        public async Task<List<Post>> GetPostsByUserId(int userId) {
            return await _context.Posts
                .Where(p => p.UserId == userId && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPublicPosts() {
            return await _context.Posts
                .Where(p => p.Visibility == "PUBLIC" && !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetPostsByHashtag(string hashtag) {
            return await _context.Posts
                .Where(p => !p.IsDeleted &&
                    p.Visibility == "PUBLIC" &&
                    p.Hashtags != null &&
                    p.Hashtags.ToLower().Contains(hashtag.ToLower()))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> SearchPosts(string keyword) {
            return await _context.Posts
                .Where(p => !p.IsDeleted &&
                    p.Visibility == "PUBLIC" &&
                    p.Content.ToLower().Contains(keyword.ToLower()))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<Post>> GetTrendingPosts() {
            DateTime since = DateTime.UtcNow.AddHours(-24);
            return await _context.Posts
                .Where(p => !p.IsDeleted &&
                       p.Visibility == "PUBLIC" &&
                       p.CreatedAt >= since)
                .OrderByDescending(p => p.LikeCount * 3 + p.CommentCount * 2 + p.ShareCount)
                .Take(20)
                .ToListAsync();
        }

        public async Task<List<Post>> GetFeedPosts(List<int> followingIds) {
            return await _context.Posts
                .Where(p => !p.IsDeleted && followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<Post> CreatePost(Post post) {
            await _context.Posts.AddAsync(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post?> UpdatePost(int postId, string content, string visibility, string? hashtags) {
            Post? post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null) return null;

            post.Content = content;
            post.Visibility = visibility;
            post.Hashtags = hashtags;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeletePost(int postId) {
            Post? post = await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted);

            if (post == null) return false;

            post.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task IncrementLikeCount(int postId) {
            await _context.Posts
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.LikeCount, x => x.LikeCount + 1));
        }

        public async Task DecrementLikeCount(int postId) {
            await _context.Posts
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.LikeCount, x => x.LikeCount - 1));
        }

        public async Task IncrementCommentCount(int postId) {
            await _context.Posts
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.CommentCount, x => x.CommentCount + 1));
        }

        public async Task DecrementCommentCount(int postId) {
            await _context.Posts
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.CommentCount, x => x.CommentCount - 1));
        }

        public async Task IncrementShareCount(int postId) {
            await _context.Posts
                .Where(p => p.PostId == postId)
                .ExecuteUpdateAsync(p => p.SetProperty(x => x.ShareCount, x => x.ShareCount + 1));
        }
    }
}