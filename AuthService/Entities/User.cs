using System.ComponentModel.DataAnnotations;

namespace AuthService.Entities {
    public class User {
        [Key]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        public string PasswordHash { get; set; } = null!;

        public string? Bio { get; set; }

        public string? AvatarUrl { get; set; }

        public bool IsPrivate { get; set; } = false;

        public bool IsActive { get; set; } = true;

        public int FollowerCount { get; set; } = 0;

        public int FollowingCount { get; set; } = 0;

        public int PostCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}