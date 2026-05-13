using System.ComponentModel.DataAnnotations;

namespace CommentService.DTOs {
    public class CreateCommentDto {
        [Required]
        public int PostId { get; set; }

        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        public string Content { get; set; } = null!;

        // Null = top-level comment; set = reply to a comment
        public int? ParentCommentId { get; set; }
    }
}
