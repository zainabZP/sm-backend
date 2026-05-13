using System.ComponentModel.DataAnnotations;

namespace FeedService.Entities {
    public class FeedItem {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int FeedOwnerId { get; set; }
        
        [Required]
        public int PostId { get; set; }
        
        [Required]
        public int PostAuthorId { get; set; }
        
        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
