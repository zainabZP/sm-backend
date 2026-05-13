using System.ComponentModel.DataAnnotations;

namespace CommentService.Entities {
    public class Comment {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int PostId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public string Content { get; set; } = null!;

        // Null = top-level comment; set = reply to another comment
        public int? ParentCommentId { get; set; }

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation properties
        public Comment? ParentComment { get; set; }
        public List<Comment> Replies { get; set; } = new();
    }
}
