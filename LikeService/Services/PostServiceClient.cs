namespace LikeService.Services {
    public class PostServiceClient {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task IncrementLikeCount(int postId) {
            await _httpClient.PutAsync($"api/posts/{postId}/increment-likes", null);
        }

        public async Task DecrementLikeCount(int postId) {
            await _httpClient.PutAsync($"api/posts/{postId}/decrement-likes", null);
        }
    }
}