using System.Security.Claims;
using AuthService.DTOs;
using AuthService.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers {
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService) {
            _authService = authService;
        }

        // only for testing if validation is working or not dont have any other use in application
        [Authorize]
        [HttpGet("validate-token")]
        public IActionResult ValidateToken() {
            return Ok(new { valid = true, message = "Token is valid" });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto) {
            AuthResponseDto? response = await _authService.Register(dto);
            if (response == null) {
                return BadRequest("Email or username already exists");
            }
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto) {
            AuthResponseDto? response = await _authService.Login(dto);
            if (response == null) {
                return Unauthorized("Invalid email or password");
            }
            return Ok(response);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout() {
            string? token = Request.Headers["Authorization"]
                .ToString()
                .Replace("Bearer ", "");

            if (string.IsNullOrEmpty(token)) {
                return BadRequest("No token provided");
            }

            bool result = await _authService.Logout(token);
            if (!result) {
                return BadRequest("Token already invalidated");
            }

            return Ok(new { message = "Logged out successfully" });
        }

        [HttpGet("users/get-user-by-id/{userId}")]
        public async Task<IActionResult> GetUserById(int userId) {
            UserResponseDto? user = await _authService.GetUserById(userId);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [HttpGet("users/search-users")]
        public async Task<IActionResult> SearchUsers([FromQuery] string q) {
            List<UserResponseDto> users = await _authService.SearchUsers(q);
            return Ok(users);
        }

        [Authorize]
        [HttpPut("users/update-profile")]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            UserResponseDto? user = await _authService.UpdateProfile(userId, dto);
            if (user == null) return NotFound("User not found");
            return Ok(user);
        }

        [Authorize]
        [HttpPut("users/change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto) {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _authService.ChangePassword(userId, dto);
            if (!result) return BadRequest("Current password is incorrect");
            return Ok(new { message = "Password changed successfully" });
        }

        [Authorize]
        [HttpPut("users/toggle-privacy")]
        public async Task<IActionResult> TogglePrivacy() {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _authService.TogglePrivacy(userId);
            if (!result) return NotFound("User not found");
            return Ok(new { message = "Privacy setting updated" });
        }

        /*
        [Authorize]
        [HttpDelete("users/deactivate")]
        public async Task<IActionResult> DeactivateAccount() {
            int userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            bool result = await _authService.DeactivateAccount(userId);
            if (!result) return NotFound("User not found");
            return Ok(new { message = "Account deactivated successfully" });
        }
        */

        [HttpGet("users/{userId}/is-private")]
        public async Task<IActionResult> IsUserPrivate(int userId) {
            bool isPrivate = await _authService.IsUserPrivate(userId);
            return Ok(isPrivate);
        }

        [HttpPost("users/get-users-by-ids")]
        public async Task<IActionResult> GetUsersByIds([FromBody] List<int> userIds) {
            List<UserResponseDto> users = await _authService.GetUsersByIds(userIds);
            return Ok(users);
        }

        [Authorize]
        [HttpGet("users/recommendations")]
        public async Task<IActionResult> GetRecommendations() {
            int currentUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            List<UserResponseDto> users = await _authService.GetRecommendedUsers(currentUserId);
            return Ok(users);
        }

        [HttpPut("users/{userId}/increment-followers")]
        public async Task<IActionResult> IncrementFollowers(int userId) {
            await _authService.IncrementFollowerCount(userId);
            return Ok();
        }

        [HttpPut("users/{userId}/decrement-followers")]
        public async Task<IActionResult> DecrementFollowers(int userId) {
            await _authService.DecrementFollowerCount(userId);
            return Ok();
        }

        [HttpPut("users/{userId}/increment-following")]
        public async Task<IActionResult> IncrementFollowing(int userId) {
            await _authService.IncrementFollowingCount(userId);
            return Ok();
        }

        [HttpPut("users/{userId}/decrement-following")]
        public async Task<IActionResult> DecrementFollowing(int userId) {
            await _authService.DecrementFollowingCount(userId);
            return Ok();
        }

        [HttpPut("users/{userId}/set-follow-counts")]
        public async Task<IActionResult> SetFollowCounts(int userId, [FromQuery] int followerCount, [FromQuery] int followingCount) {
            await _authService.SetFollowCounts(userId, followerCount, followingCount);
            return Ok();
        }
    }
}