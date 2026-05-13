namespace PostService.Events {
    public class PostCreatedEvent {
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
