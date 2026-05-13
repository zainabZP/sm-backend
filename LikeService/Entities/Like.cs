using System.ComponentModel.DataAnnotations;

namespace LikeService.Entities {
    public class Like {
        [Key]
        public int LikeId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int TargetId { get; set; }

        [Required]
        public string TargetType { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}