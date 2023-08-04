using ChatService.Api.Hubs;
using ChatService.Application.Models;
using Dncy.Tools.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Localization;
using System.ComponentModel.DataAnnotations;
using ChatService.Api.Models;
using Dncy.EventBus.RabbitMQ;
using ChatService.Application.IntegrationEvents.Events;
using Dncy.EventBus.Abstract.Models;
using Dncy.StackExchangeRedis;

namespace ChatService.Api.Controllers
{
    [Route("api/[controller]/[action]")]
    [AutoResolveDependency]
    [ApiController]
    [AllowAnonymous]
    public partial class HomeController : ControllerBase, IResponseWraps
    {
        [AutoInject]
        private readonly IStringLocalizer<SharedResource> _stringLocalizer;


        [AutoInject]
        private readonly ILogger<HomeController> _logger;

        [AutoInject]
        private readonly IHubContext<ChatHub> _chatHub;

        [AutoInject]
        private readonly EventBusRabbitMQ _bus;

        [AutoInject]
        private readonly RedisClientFactory _redisFactory;

        [HttpGet]
        public async Task<ResultDto> SendMsg(string name)
        {
            await _chatHub.Clients.All.SendAsync("message", name);
            return this.Success<string>("ok");
        }


        [HttpGet]
        public async Task<ResultDto> SendToUser(string userId,string toUserId,string message)
        {
            var user = await _redisFactory["docker01"].Db.HashGetAsync(ChatHub.REDIS_CACHE_KEY, toUserId);
            if (string.IsNullOrEmpty(user))
            {
                return this.ErrorRequest("目标用户不存在");
            }
            var userModel = JsonConvert.DeserializeObject<SignalRSessionModel>(user);
            var @event=new ChatMessageIntegrationEvent(userId,toUserId,message);
            @event.RouteKey = "handle_msg";
            await _bus.PublishAsync(@event);
            return this.Success<string>("ok");
        }

        [HttpGet]
        [Authorize]
        public async Task<ResultDto> OnLineUsers()
        {
            var user = await _redisFactory["docker01"].Db.HashGetAllAsync(ChatHub.REDIS_CACHE_KEY);
            return this.Success(user);
        }

        [HttpGet]
        public async Task<ResultDto> TestRedis([FromServices]RedisClientFactory factory)
        {
            var client = factory["docker01"];
            await client.Db.StringSetAsync("hello", "workd", TimeSpan.FromDays(1));
            var res = await client.Db.StringGetAsync("hello");
            Debug.Assert(!string.IsNullOrEmpty(res));
            return this.Success(res);
        }
        
        [HttpGet]
        public async Task<ResultDto> TestMq([FromServices]EventBusRabbitMQ bus)
        {
            await bus.PublishAsync(new ChatMessageIntegrationEvent("a","b","ccc")
            {
                RouteKey = "handle_msg"
            });
            return this.Success("ok");
        }
    }

}

