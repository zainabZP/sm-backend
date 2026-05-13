using System.ComponentModel.DataAnnotations;

namespace FollowService.Entities {
    public class Follow {
        [Key]
        public int FollowId { get; set; }

        [Required]
        public int FollowerId { get; set; }

        [Required]
        public int FolloweeId { get; set; }

        public string Status { get; set; } = "PENDING";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}