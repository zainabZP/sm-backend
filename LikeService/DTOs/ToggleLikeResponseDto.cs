namespace LikeService.DTOs {
    public class ToggleLikeResponseDto {
        public bool IsLiked { get; set; }
        public string Message { get; set; } = null!;
    }
}