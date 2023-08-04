namespace ChatService.Domain
{
    public static class DependencyInject
    {
        public static IServiceCollection AddDomainModule(this IServiceCollection service)
        {
            service.AutoInjectChatService_Domain();
            return service;
        }
    }
}