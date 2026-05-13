namespace PostService.DTOs {
    public class PostResponseDto {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = null!;
        public string? MediaUrl { get; set; }
        public string? MediaType { get; set; }
        public string Visibility { get; set; } = null!;
        public string? Hashtags { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public int ShareCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsUserPrivate { get; set; }
    }
}