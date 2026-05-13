using System.Security.Claims;
using FollowService.DTOs;
using FollowService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FollowService.Controllers {
    [ApiController]
    [Route("api/follows")]
    public class FollowController : ControllerBase {
        private readonly IFollowService _followService;

        public FollowController(IFollowService followService) {
            _followService = followService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> FollowUser(FollowRequestDto dto) {
            int followerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            FollowResponseDto? response = await _followService.FollowUser(followerId, dto.FolloweeId);
            if (response == null) return BadRequest("Already following or invalid request");
            return Ok(response);
        }

        [Authorize]
        [HttpDelete("{followeeId}")]
        public async Task<IActionResult> UnfollowUser(int followeeId) {
            int followerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _followService.UnfollowUser(followerId, followeeId);
            if (!result) return NotFound("Follow relationship not found");
            return Ok(new { message = "Unfollowed successfully" });
        }

        [Authorize]
        [HttpPut("{followId}/accept")]
        public async Task<IActionResult> AcceptFollowRequest(int followId) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _followService.AcceptFollowRequest(followId, userId);
            if (!result) return BadRequest("Request not found or unauthorized");
            return Ok("Follow request accepted");
        }

        [Authorize]
        [HttpPut("{followId}/reject")]
        public async Task<IActionResult> RejectFollowRequest(int followId) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _followService.RejectFollowRequest(followId, userId);
            if (!result) return BadRequest("Request not found or unauthorized");
            return Ok("Follow request rejected");
        }

        [HttpGet("{userId}/followers")]
        public async Task<IActionResult> GetFollowers(int userId) {
            List<FollowResponseDto> followers = await _followService.GetFollowers(userId);
            return Ok(followers);
        }

        [HttpGet("{userId}/following")]
        public async Task<IActionResult> GetFollowing(int userId) {
            List<FollowResponseDto> following = await _followService.GetFollowing(userId);
            return Ok(following);
        }

        [Authorize]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingRequests() {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<FollowResponseDto> pending = await _followService.GetPendingRequests(userId);
            return Ok(pending);
        }

        [Authorize]
        [HttpGet("is-following/{followeeId}")]
        public async Task<IActionResult> IsFollowing(int followeeId) {
            int followerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            FollowResponseDto? result = await _followService.IsFollowing(followerId, followeeId);
            return Ok(result);
        }

        /*
        [HttpGet("check")]
        public async Task<IActionResult> CheckFollowStatus([FromQuery] int followerId, [FromQuery] int followeeId) {
            FollowResponseDto? result = await _followService.IsFollowing(followerId, followeeId);
            return Ok(new { isFollowing = result?.Status == "ACCEPTED" });
        }
        */

        [Authorize]
        [HttpPost("sync")]
        public async Task<IActionResult> SyncCounts() {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _followService.SyncUserCounts(userId);
            return Ok("Follow counts synchronized");
        }

        [HttpGet("{userId}/follower-count")]
        public async Task<IActionResult> GetFollowerCount(int userId) {
            int count = await _followService.GetFollowerCount(userId);
            return Ok(count);
        }

        [HttpGet("{userId}/following-count")]
        public async Task<IActionResult> GetFollowingCount(int userId) {
            int count = await _followService.GetFollowingCount(userId);
            return Ok(count);
        }
    }
}