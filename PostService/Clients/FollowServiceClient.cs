using System.Net.Http.Json;

namespace PostService.Clients {
    public class FollowServiceClient {
        private readonly HttpClient _httpClient;

        public FollowServiceClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task<bool> IsFollowing(int followerId, int followeeId) {
            try {
                var response = await _httpClient.GetAsync($"/api/follows/check?followerId={followerId}&followeeId={followeeId}");
                if (response.IsSuccessStatusCode) {
                    var result = await response.Content.ReadFromJsonAsync<CheckFollowResponse>();
                    return result?.IsFollowing ?? false;
                }
                return false;
            } catch {
                return false;
            }
        }

        private class CheckFollowResponse {
            public bool IsFollowing { get; set; }
        }
    }
}
