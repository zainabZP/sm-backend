namespace LikeService.DTOs {
    public class ToggleLikeDto {
        public int TargetId { get; set; }
        public string TargetType { get; set; } = null!;
    }
}