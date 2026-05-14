using System.Net.Http.Json;
using FeedService.DTOs;

namespace FeedService.Clients {
    public class PostServiceClient {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient, IConfiguration configuration) {
            _httpClient = httpClient;
            var url = configuration["ServiceUrls:PostService"] ?? configuration["ServiceUrls__PostService"];

            if (string.IsNullOrEmpty(url) || url == "SET_BY_ENV") {
                throw new Exception($"PostService URL is not configured. Key: ServiceUrls:PostService. Value received: '{url}'");
            }

            _httpClient.BaseAddress = new Uri(url);
        }

        public async Task<List<PostResponseDto>> GetFeedPosts(List<int> followingIds) {
            if (followingIds == null || followingIds.Count == 0) return new List<PostResponseDto>();

            var query = string.Join("&", followingIds.Select(id => $"followingIds={id}"));
            try {
                var response = await _httpClient.GetFromJsonAsync<List<PostResponseDto>>($"/api/posts/feed?{query}");
                return response ?? new List<PostResponseDto>();
            } catch {
                return new List<PostResponseDto>();
            }
        }

        public async Task<List<PostResponseDto>> GetPublicPosts() {
            try {
                var response = await _httpClient.GetFromJsonAsync<List<PostResponseDto>>("/api/posts/public");
                return response ?? new List<PostResponseDto>();
            } catch {
                return new List<PostResponseDto>();
            }
        }

        public async Task<List<PostResponseDto>> GetPostsByIds(List<int> postIds) {
            if (postIds == null || postIds.Count == 0) return new List<PostResponseDto>();

            try {
                var response = await _httpClient.PostAsJsonAsync("/api/posts/batch", postIds);
                if (response.IsSuccessStatusCode) {
                    return await response.Content.ReadFromJsonAsync<List<PostResponseDto>>() ?? new List<PostResponseDto>();
                }
                return new List<PostResponseDto>();
            } catch {
                return new List<PostResponseDto>();
            }
        }
    }
}
