using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace FollowService.Services {
    public class AuthServiceClient {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<bool> IsUserPrivate(int userId) {
            HttpResponseMessage response = await _httpClient
                .GetAsync($"api/auth/users/{userId}/is-private");
            if (!response.IsSuccessStatusCode) return false;
            string content = await response.Content.ReadAsStringAsync();
            return bool.Parse(content);
        }

        public async Task IncrementFollowerCount(int userId) {
            await _httpClient.PutAsync($"api/auth/users/{userId}/increment-followers", null);
        }

        public async Task DecrementFollowerCount(int userId) {
            await _httpClient.PutAsync($"api/auth/users/{userId}/decrement-followers", null);
        }

        public async Task IncrementFollowingCount(int userId) {
            await _httpClient.PutAsync($"api/auth/users/{userId}/increment-following", null);
        }

        public async Task DecrementFollowingCount(int userId) {
            await _httpClient.PutAsync($"api/auth/users/{userId}/decrement-following", null);
        }

        public async Task UpdateFollowCounts(int userId, int followerCount, int followingCount) {
            await _httpClient.PutAsync($"api/auth/users/{userId}/set-follow-counts?followerCount={followerCount}&followingCount={followingCount}", null);
        }

        public async Task<List<UserInfo>> GetUsersByIds(List<int> userIds) {
            try {
                var response = await _httpClient.PostAsJsonAsync("api/auth/users/get-users-by-ids", userIds);
                if (response.IsSuccessStatusCode) {
                    return await response.Content.ReadFromJsonAsync<List<UserInfo>>() ?? new List<UserInfo>();
                }
                return new List<UserInfo>();
            } catch {
                return new List<UserInfo>();
            }
        }

        public class UserInfo {
            [JsonPropertyName("userId")]
            public int UserId { get; set; }

            [JsonPropertyName("userName")]
            public string UserName { get; set; } = null!;

            [JsonPropertyName("fullName")]
            public string FullName { get; set; } = null!;

            [JsonPropertyName("avatarUrl")]
            public string? AvatarUrl { get; set; }
        }
    }
}