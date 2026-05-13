using System.Text;
using System.Text.Json;
using PostService.Events;
using PostService.Interfaces;
using RabbitMQ.Client;

namespace PostService.Services {
    public class RabbitMQPublisher : IRabbitMQPublisher, IAsyncDisposable {
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = "post_events";
        private readonly string _routingKey = "post.created";

        public RabbitMQPublisher(IConfiguration configuration) {
            _configuration = configuration;
        }

        private async Task Initialize() {
            if (_channel != null) return;

            var factory = new ConnectionFactory {
                HostName = _configuration["RabbitMQ:Host"] ?? "localhost",
                UserName = _configuration["RabbitMQ:Username"] ?? "guest",
                Password = _configuration["RabbitMQ:Password"] ?? "guest"
            };

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic);
        }

        public async Task PublishPostCreated(PostCreatedEvent @event) {
            try {
                await Initialize();
                if (_channel == null) return;

                var message = JsonSerializer.Serialize(@event);
                var body = Encoding.UTF8.GetBytes(message);

                await _channel.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: _routingKey,
                    body: body
                );
            } catch (Exception ex) {
                // Log the error but don't break the main flow
                Console.WriteLine($"Error publishing to RabbitMQ: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync() {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
        }
    }
}
