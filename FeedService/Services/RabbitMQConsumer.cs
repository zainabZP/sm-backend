using System.Text;
using System.Text.Json;
using FeedService.Clients;
using FeedService.Entities;
using FeedService.Events;
using FeedService.Interfaces;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FeedService.Services {
    public class RabbitMQConsumer : BackgroundService {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumer> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = "post_events";
        private readonly string _queueName = "feed_service_queue";
        private readonly string _routingKey = "post.created";

        public RabbitMQConsumer(
            IConfiguration configuration,
            IServiceProvider serviceProvider,
            ILogger<RabbitMQConsumer> logger) {
            _configuration = configuration;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await InitializeRabbitMQ();

            var consumer = new AsyncEventingBasicConsumer(_channel!);
            consumer.ReceivedAsync += async (model, ea) => {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var @event = JsonSerializer.Deserialize<PostCreatedEvent>(message);

                if (@event != null) {
                    _logger.LogInformation("Processing post created event for PostId: {PostId}", @event.PostId);
                    await ProcessFeedFanout(@event);
                }

                await _channel!.BasicAckAsync(ea.DeliveryTag, false);
            };

            await _channel!.BasicConsumeAsync(_queueName, false, consumer);

            while (!stoppingToken.IsCancellationRequested) {
                await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task InitializeRabbitMQ() {
            var factory = new ConnectionFactory();
            
            if (!string.IsNullOrEmpty(_configuration["RabbitMQ:Uri"])) {
                factory.Uri = new Uri(_configuration["RabbitMQ:Uri"]!);
            } else {
                factory.HostName = _configuration["RabbitMQ:Host"] ?? "localhost";
                factory.UserName = _configuration["RabbitMQ:Username"] ?? "guest";
                factory.Password = _configuration["RabbitMQ:Password"] ?? "guest";
            }

            _connection = await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();

            await _channel.ExchangeDeclareAsync(_exchangeName, ExchangeType.Topic);
            await _channel.QueueDeclareAsync(_queueName, true, false, false, null);
            await _channel.QueueBindAsync(_queueName, _exchangeName, _routingKey);
        }

        private async Task ProcessFeedFanout(PostCreatedEvent @event) {
            using var scope = _serviceProvider.CreateScope();
            var followClient = scope.ServiceProvider.GetRequiredService<FollowServiceClient>();
            var feedRepository = scope.ServiceProvider.GetRequiredService<IFeedRepository>();

            try {
                // 1. Get followers of the author
                var followerIds = await followClient.GetFollowerUserIds(@event.UserId);
                
                // Add author to their own feed
                followerIds.Add(@event.UserId);

                // 2. Create feed items
                var feedItems = followerIds.Select(followerId => new FeedItem {
                    FeedOwnerId = followerId,
                    PostId = @event.PostId,
                    PostAuthorId = @event.UserId,
                    CreatedAt = @event.CreatedAt
                }).ToList();

                // 3. Save to repository (Idempotency should be handled in repository if possible)
                await feedRepository.SaveFeedItems(feedItems);
                
                _logger.LogInformation("Successfully fanned out post {PostId} to {Count} feeds", @event.PostId, feedItems.Count);
            } catch (Exception ex) {
                _logger.LogError(ex, "Error processing feed fanout for post {PostId}", @event.PostId);
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken) {
            if (_channel != null) await _channel.CloseAsync();
            if (_connection != null) await _connection.CloseAsync();
            await base.StopAsync(cancellationToken);
        }
    }
}
