using ChatService.Api.Hubs;
using ChatService.Api.Models;
using ChatService.Application.IntegrationEvents.Events;
using ChatService.Infra.Constants;
using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.SubscribeActivator;
using Dncy.StackExchangeRedis;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging.Abstractions;

namespace ChatService.Api.SubscribeHandlers
{
    public class ChatMessageHandler: IntegrationEventHandler
    {
        private readonly ILogger<ChatMessageHandler> _logger;

        private readonly IHubContext<ChatHub> _chatHub;
        private readonly IConfiguration _configuration;

        private readonly RedisClientFactory _redisClientFactory;

        public ChatMessageHandler(
            IHubContext<ChatHub> chatHub,
            IConfiguration configuration,
            RedisClientFactory redisClientFactory,
            ILogger<ChatMessageHandler> logger = null)
        {
            _logger = logger ?? NullLogger<ChatMessageHandler>.Instance;
            _chatHub = chatHub;
            _configuration = configuration;
            _redisClientFactory = redisClientFactory;
        }

        private async Task SendMessage(ChatMessageIntegrationEvent @event)
        {
            var user = await _redisClientFactory["docker01"].Db.HashGetAsync(ChatHub.REDIS_CACHE_KEY, @event.ToUser);
            if (!string.IsNullOrEmpty(user))
            {
                var userModel = JsonConvert.DeserializeObject<SignalRSessionModel>(user);
                if (userModel.ServiceInstrance==EnvironmentConstants.EnvHostName)
                {
                    await _chatHub.Clients.Clients(userModel.ConnectionId).SendAsync("message", @event.User,@event.Message);
                }
            }
        }

        [Subscribe("handle_msg",nameof(EventBusRabbitMQ))]
        public async Task UserEnableEventHandler(ChatMessageIntegrationEvent customMessage)
        {
            _logger.LogInformation($"收到消息[s1] : {customMessage?.User}");
            await SendMessage(customMessage);
        }
        
        // [Subscribe("handle_msg_s2",nameof(EventBusRabbitMQ))]
        // public async Task UserEnableEventHandlerS2(ChatMessageIntegrationEvent customMessage)
        // {
        //     _logger.LogInformation($"收到消息[s2]  : {customMessage?.User}");
        //     await SendMessage(customMessage);
        // }
        //
        // [Subscribe("handle_msg_s3",nameof(EventBusRabbitMQ))]
        // public async Task UserEnableEventHandlerS3(ChatMessageIntegrationEvent customMessage)
        // {
        //     _logger.LogInformation($"收到消息[s3]  : {customMessage?.User}");
        //     await SendMessage(customMessage);
        // }
    }
}