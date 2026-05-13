using System.ComponentModel.DataAnnotations;

namespace AuthService.Entities {
    public class BlacklistedToken {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public string Token { get; set; } = null!;
        
        public DateTime BlacklistedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime ExpiresAt { get; set; }
    }
}