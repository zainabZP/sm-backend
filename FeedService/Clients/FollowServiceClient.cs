using System.Net.Http.Json;
using FeedService.DTOs;

namespace FeedService.Clients {
    public class FollowServiceClient {
        private readonly HttpClient _httpClient;

        public FollowServiceClient(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(configuration["ServiceUrls:FollowService"]!);
        }

        public async Task<List<int>> GetFollowingUserIds(int userId) {
            try {
                var response = await _httpClient.GetFromJsonAsync<List<FollowResponseDto>>($"/api/follows/{userId}/following");
                return response?.Select(f => f.FolloweeId).ToList() ?? new List<int>();
            } catch {
                return new List<int>();
            }
        }

        public async Task<List<int>> GetFollowerUserIds(int userId) {
            try {
                var response = await _httpClient.GetFromJsonAsync<List<FollowResponseDto>>($"/api/follows/{userId}/followers");
                return response?.Select(f => f.FollowerId).ToList() ?? new List<int>();
            } catch {
                return new List<int>();
            }
        }
    }
}
