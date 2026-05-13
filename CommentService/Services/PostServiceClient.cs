namespace CommentService.Services {
    public class PostServiceClient {
        private readonly HttpClient _httpClient;

        public PostServiceClient(HttpClient httpClient) {
            _httpClient = httpClient;
        }

        public async Task IncrementCommentCount(int postId) {
            await _httpClient.PutAsync($"api/posts/{postId}/increment-comments", null);
        }

        public async Task DecrementCommentCount(int postId) {
            await _httpClient.PutAsync($"api/posts/{postId}/decrement-comments", null);
        }
    }
}
