namespace LikeService.DTOs {
    public class LikeResponseDto {
        public int LikeId { get; set; }
        public int UserId { get; set; }
        public int TargetId { get; set; }
        public string TargetType { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}