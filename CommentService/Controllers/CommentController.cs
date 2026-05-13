using System.Security.Claims;
using CommentService.DTOs;
using CommentService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CommentService.Controllers {
    [ApiController]
    [Route("api/comments")]
    public class CommentController : ControllerBase {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService commentService) {
            _commentService = commentService;
        }

        /// <summary>
        /// Add a comment to a post or a reply to a comment.
        /// Set ParentCommentId to reply to a comment (supports unlimited nesting).
        /// </summary>
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddComment([FromBody] CreateCommentDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            try {
                CommentResponseDto comment = await _commentService.AddComment(userId, dto);
                return Ok(comment);
            } catch (InvalidOperationException ex) {
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Get all comments for a post as a nested tree (replies inside each comment).
        /// </summary>
        [HttpGet("post/{postId}")]
        public async Task<IActionResult> GetCommentsByPost(int postId) {
            List<CommentResponseDto> comments = await _commentService.GetCommentsByPost(postId);
            return Ok(comments);
        }

        /// <summary>
        /// Get all posts the authenticated user has commented on.
        /// </summary>
        [Authorize]
        [HttpGet("my-posts")]
        public async Task<IActionResult> GetPostsCommentedByCurrentUser() {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<CommentedPostDto> posts = await _commentService.GetPostsCommentedByUser(userId);
            return Ok(posts);
        }

        /// <summary>
        /// Get all posts a specific user has commented on.
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetPostsCommentedByUser(int userId) {
            List<CommentedPostDto> posts = await _commentService.GetPostsCommentedByUser(userId);
            return Ok(posts);
        }

        /// <summary>
        /// Delete own comment (and all its nested replies) by soft-delete.
        /// </summary>
        [Authorize]
        [HttpDelete("{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _commentService.DeleteComment(commentId, userId);
            if (!result) return NotFound("Comment not found or you are not the owner.");
            return Ok("Comment deleted successfully.");
        }
    }
}
