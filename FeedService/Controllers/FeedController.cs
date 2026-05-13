using System.Security.Claims;
using FeedService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FeedService.Controllers {
    [ApiController]
    [Route("api/feed")]
    public class FeedController : ControllerBase {
        private readonly IFeedService _feedService;

        public FeedController(IFeedService feedService) {
            _feedService = feedService;
        }

        [Authorize]
        [HttpGet("following")]
        public async Task<IActionResult> GetFollowingFeed() {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString)) return Unauthorized();

            int userId = int.Parse(userIdString);
            var feed = await _feedService.GetFollowingFeed(userId);
            return Ok(feed);
        }

        [HttpGet("explore")]
        public async Task<IActionResult> GetExploreFeed() {
            var feed = await _feedService.GetExploreFeed();
            return Ok(feed);
        }
    }
}
