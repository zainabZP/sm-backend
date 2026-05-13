using PostService.Events;

namespace PostService.Interfaces {
    public interface IRabbitMQPublisher {
        Task PublishPostCreated(PostCreatedEvent @event);
    }
}
