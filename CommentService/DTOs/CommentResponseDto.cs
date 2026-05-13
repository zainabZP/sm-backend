namespace CommentService.DTOs {
    public class CommentResponseDto {
        public int CommentId { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public string Content { get; set; } = null!;
        public int? ParentCommentId { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // Nested replies — recursively populated
        public List<CommentResponseDto> Replies { get; set; } = new();
    }
}
