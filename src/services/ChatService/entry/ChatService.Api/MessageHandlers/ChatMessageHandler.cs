using ChatService.Api.Hubs;
using ChatService.Application.IntegrationEvents.Events;
using Dncy.EventBus.SubscribeActivator;
using Microsoft.AspNetCore.SignalR;

namespace ChatService.Api.MessageHandlers
{
    // public class ChatMessageHandler:IntegrationEventHandler
    // {
    //     private readonly IHubContext<ChatHub> _chatHub;
    //
    //     public ChatMessageHandler(IHubContext<ChatHub> chatHub)
    //     {
    //         _chatHub = chatHub;
    //     }
    //
    //
    //     /// <summary>
    //     /// 服务器001上的实例1
    //     /// </summary>
    //     /// <param name="msg"></param>
    //     /// <returns></returns>
    //     [Subscribe("/server001/s1")]
    //     public async Task ReceiveHubMessage(ChatMessageIntegrationEvent msg)
    //     {
    //         var user = msg.ToUser;
    //         if (!InMemorySession.Users.TryGetValue(user, out var session))
    //         {
    //             return;
    //         }
    //         await _chatHub.Clients.Client(session.ConnectionId).SendAsync("message", msg.Message);
    //     }
    // }
}