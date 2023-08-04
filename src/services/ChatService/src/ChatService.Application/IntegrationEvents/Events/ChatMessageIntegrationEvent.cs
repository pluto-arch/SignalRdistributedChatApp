using Dncy.EventBus.Abstract.Models;

namespace ChatService.Application.IntegrationEvents.Events
{
    public class ChatMessageIntegrationEvent:IntegrationEvent
    {
        public string User { get;  set; }
        public string ToUser { get;  set; }
        public string Message { get;  set; }

        public ChatMessageIntegrationEvent(string user,string toUser,string message)
        {
            User = user;
            ToUser = toUser;
            Message = message;
        }

        public ChatMessageIntegrationEvent()
        {
            
        }
    }
}