using ChatService.Api.Models;
using ChatService.Infra.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Polly;
using System.Collections.Concurrent;
using ChatService.Infra.Constants;
using Dncy.StackExchangeRedis;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace ChatService.Api.Hubs
{

    public static class InMemorySession
    {
        public static ConcurrentDictionary<string, SignalRSessionModel> Users = new();

    }


    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {

        private readonly RedisClientFactory _redisFactory;
        private readonly IConfiguration _configuration;

        public const string REDIS_CACHE_KEY = "CHAT_APP";
        
        
        public ChatHub(RedisClientFactory factory,IConfiguration configuration)
        {
            _redisFactory = factory;
            _configuration = configuration; 
        }
        
        
        public override Task OnConnectedAsync()
        {
            var redis = _redisFactory["docker01"];
            var connId = Context.ConnectionId;
            var userId = Context.UserIdentifier;
            var newValue = new SignalRSessionModel
            {
                MechineName = Environment.MachineName,
                ServiceInstrance = EnvironmentConstants.EnvHostName,
                ConnectionId = connId
            };

            Context.ConnectionAborted.Register(async () =>
            {
                await redis.Db.HashDeleteAsync(REDIS_CACHE_KEY, userId);
            });

            redis.Db.HashSetAsync(REDIS_CACHE_KEY, userId, JsonConvert.SerializeObject(newValue));
            return base.OnConnectedAsync();
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var redis = _redisFactory["docker01"];
            var userId = Context.UserIdentifier;
            await redis.Db.HashDeleteAsync(REDIS_CACHE_KEY, userId);
            await base.OnDisconnectedAsync(exception);
        }
    }
}