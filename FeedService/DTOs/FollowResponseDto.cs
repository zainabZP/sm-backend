namespace FeedService.DTOs {
    public class FollowResponseDto {
        public int FollowId { get; set; }
        public int FollowerId { get; set; }
        public int FolloweeId { get; set; }
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
