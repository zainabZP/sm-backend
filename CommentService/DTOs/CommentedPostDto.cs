namespace CommentService.DTOs {
    public class CommentedPostDto {
        public int PostId { get; set; }
        public DateTime FirstCommentedAt { get; set; }
        public DateTime LastCommentedAt { get; set; }
        public int CommentCount { get; set; }
    }
}
