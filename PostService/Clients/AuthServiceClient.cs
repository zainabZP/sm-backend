using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace PostService.Clients {
    public class AuthServiceClient {
        private readonly HttpClient _httpClient;

        public AuthServiceClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<bool> IsUserPrivate(int userId) {
            try {
                var response = await _httpClient.GetAsync($"api/auth/users/{userId}/is-private");
                if (response.IsSuccessStatusCode) {
                    return await response.Content.ReadFromJsonAsync<bool>();
                }
                return false;
            } catch {
                return false;
            }
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

            [JsonPropertyName("isPrivate")]
            public bool IsPrivate { get; set; }
        }
    }
}
