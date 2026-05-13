using System.ComponentModel.DataAnnotations;

namespace PostService.Entities {
    public class Post {
        [Key]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        public string? MediaUrl { get; set; }

        public string? MediaType { get; set; }

        public string Visibility { get; set; } = "PUBLIC";

        public string? Hashtags { get; set; }

        public int LikeCount { get; set; } = 0;

        public int CommentCount { get; set; } = 0;

        public int ShareCount { get; set; } = 0;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}