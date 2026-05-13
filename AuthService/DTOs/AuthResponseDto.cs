namespace AuthService.DTOs {
    public class AuthResponseDto {
        public string Token { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public int UserId { get; set; }
    }
}