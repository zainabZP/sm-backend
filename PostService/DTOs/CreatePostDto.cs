namespace PostService.DTOs {
    public class CreatePostDto {
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public string Visibility { get; set; } = "PUBLIC";
        public string? Hashtags { get; set; }
    }
}