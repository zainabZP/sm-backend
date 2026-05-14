using System.Net.Http.Json;
using FeedService.DTOs;

namespace FeedService.Clients {
    public class FollowServiceClient {
        private readonly HttpClient _httpClient;

        public FollowServiceClient(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            var url = configuration["ServiceUrls:FollowService"] ?? configuration["ServiceUrls__FollowService"];
            
            if (string.IsNullOrEmpty(url) || url == "SET_BY_ENV") {
                // Fallback to a default if absolutely necessary, but better to throw a clear error
                throw new Exception($"FollowService URL is not configured. Key: ServiceUrls:FollowService. Value received: '{url}'");
            }

            _httpClient.BaseAddress = new Uri(url);
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
