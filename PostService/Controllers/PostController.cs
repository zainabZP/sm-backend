using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PostService.DTOs;
using PostService.Interfaces;

namespace PostService.Controllers {
    [ApiController]
    [Route("api/posts")]
    public class PostController : ControllerBase {
        private readonly IPostService _postService;

        public PostController(IPostService postService) {
            _postService = postService;
        }

        [HttpGet("public")]
        public async Task<IActionResult> GetPublicPosts() {
            List<PostResponseDto> posts = await _postService.GetPublicPosts();
            return Ok(posts);
        }

        /*
        [HttpGet("trending")]
        public async Task<IActionResult> GetTrendingPosts() {
            List<PostResponseDto> posts = await _postService.GetTrendingPosts();
            return Ok(posts);
        }
        */

        [HttpGet("hashtag/{hashtag}")]
        public async Task<IActionResult> GetPostsByHashtag(string hashtag) {
            List<PostResponseDto> posts = await _postService.GetPostsByHashtag(hashtag);
            return Ok(posts);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchPosts([FromQuery] string keyword) {
            List<PostResponseDto> posts = await _postService.SearchPosts(keyword);
            return Ok(posts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(int postId) {
            int? currentUserId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null) currentUserId = int.Parse(userIdClaim);

            PostResponseDto? post = await _postService.GetPostById(postId, currentUserId);
            if (post == null) return NotFound("Post not found");
            return Ok(post);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsByUserId(int userId) {
            int? currentUserId = null;
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userIdClaim != null) currentUserId = int.Parse(userIdClaim);
            
            List<PostResponseDto> posts = await _postService.GetPostsByUserId(userId, currentUserId);
            return Ok(posts);
        }

        [HttpGet("feed")]
        public async Task<IActionResult> GetFeedPosts([FromQuery] List<int> followingIds) {
            List<PostResponseDto> posts = await _postService.GetFeedPosts(followingIds);
            return Ok(posts);
        }

        [HttpPost("batch")]
        public async Task<IActionResult> GetPostsByIds([FromBody] List<int> postIds) {
            List<PostResponseDto> posts = new List<PostResponseDto>();
            foreach (var id in postIds) {
                var post = await _postService.GetPostById(id);
                if (post != null) posts.Add(post);
            }
            return Ok(posts);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreatePost(CreatePostDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            PostResponseDto post = await _postService.CreatePost(userId, dto);
            return Ok(post);
        }

        [Authorize]
        [HttpPut("{postId}")]
        public async Task<IActionResult> UpdatePost(int postId, UpdatePostDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            PostResponseDto? post = await _postService.UpdatePost(postId, userId, dto);
            if (post == null) return NotFound("Post not found or you are not the owner");
            return Ok(post);
        }

        [Authorize]
        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _postService.DeletePost(postId, userId);
            if (!result) return NotFound("Post not found or you are not the owner");
            return Ok("Post deleted successfully");
        }

        /*
        [Authorize]
        [HttpPost("{postId}/share")]
        public async Task<IActionResult> SharePost(int postId) {
            await _postService.IncrementShareCount(postId);
            return Ok("Post shared successfully");
        }
        */

        [HttpPut("{postId}/increment-likes")]
        public async Task<IActionResult> IncrementLikes(int postId) {
            await _postService.IncrementLikeCount(postId);
            return Ok();
        }

        [HttpPut("{postId}/decrement-likes")]
        public async Task<IActionResult> DecrementLikes(int postId) {
            await _postService.DecrementLikeCount(postId);
            return Ok();
        }

        [HttpPut("{postId}/increment-comments")]
        public async Task<IActionResult> IncrementComments(int postId) {
            await _postService.IncrementCommentCount(postId);
            return Ok();
        }

        [HttpPut("{postId}/decrement-comments")]
        public async Task<IActionResult> DecrementComments(int postId) {
            await _postService.DecrementCommentCount(postId);
            return Ok();
        }
    }
}