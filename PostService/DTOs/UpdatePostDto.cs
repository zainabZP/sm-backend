namespace PostService.DTOs {
    public class UpdatePostDto {
        public string Content { get; set; } = null!;
        public string Visibility { get; set; } = "PUBLIC";
        public string? Hashtags { get; set; }
    }
}