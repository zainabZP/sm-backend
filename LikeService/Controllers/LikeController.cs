using System.Security.Claims;
using LikeService.DTOs;
using LikeService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LikeService.Controllers {
    [ApiController]
    [Route("api/likes")]
    public class LikeController : ControllerBase {
        private readonly ILikeService _likeService;

        public LikeController(ILikeService likeService) {
            _likeService = likeService;
        }

        [Authorize]
        [HttpPost("toggle")]
        public async Task<IActionResult> ToggleLike(ToggleLikeDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool liked = await _likeService.ToggleLike(userId, dto);
            return Ok(new { status = liked ? "Liked" : "Unliked", liked });
        }

        /*
        [HttpGet("target/{targetId}")]
        public async Task<IActionResult> GetLikesByTarget(int targetId, [FromQuery] string targetType) {
            List<LikeResponseDto> likes = await _likeService.GetLikesByTarget(targetId, targetType);
            return Ok(likes);
        }
        */

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetLikesByUser(int userId) {
            List<LikeResponseDto> likes = await _likeService.GetLikesByUser(userId);
            return Ok(likes);
        }

        [HttpGet("count/{targetId}")]
        public async Task<IActionResult> GetLikeCount(int targetId, [FromQuery] string targetType) {
            int count = await _likeService.GetLikeCount(targetId, targetType);
            return Ok(count);
        }

        [Authorize]
        [HttpGet("has-liked/{targetId}")]
        public async Task<IActionResult> HasUserLiked(int targetId, [FromQuery] string targetType) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool hasLiked = await _likeService.HasUserLiked(userId, targetId, targetType);
            return Ok(hasLiked);
        }
    }
}