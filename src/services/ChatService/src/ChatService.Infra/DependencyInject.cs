using ChatService.Domain.Infra;
using ChatService.Infra.Constants;
using ChatService.Infra.EntityFrameworkCore;
using ChatService.Infra.Global;
using ChatService.Infra.Providers;
using Dncy.EventBus.RabbitMQ;
using Dncy.EventBus.RabbitMQ.Connection;
using Dncy.EventBus.RabbitMQ.Options;
using Dncy.EventBus.SubscribeActivator;
using Dncy.StackExchangeRedis;
using RabbitMQ.Client;
using StackExchange.Redis;

namespace ChatService.Infra
{
    public static class DependencyInject
    {
        public static IServiceCollection AddInfraModule(this IServiceCollection service, IConfiguration configuration)
        {

            service.AddSingleton<GlobalAccessor.CurrentUserAccessor>();
            service.AddTransient<GlobalAccessor.CurrentUser>();

            var ctxs = GetDbContextTypes();

            service.AddTransient<IDomainEventDispatcher, MediatrDomainEventDispatcher>();
            service.AddEfCoreInfraComponent(configuration, ctxs);
            service.AddEfUnitofWork(ctxs);

            AddRedis(service);
            AddRabbitMqEventBus(service,configuration);
            return service;
        }


        public static List<Type> GetDbContextTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes().Where(x => x.IsAssignableTo(typeof(DbContext)) && !x.IsAbstract && !x.Name.Contains("Migration")).ToList();
        }


        static void AddRedis(IServiceCollection services)
        {
            services.AddRedisClient(o =>
            {
                o.CommandMap = CommandMap.Default;
                o.DefaultDatabase = 0;
                o.ClientName = "docker01";
                o.Password = "redispw";
                o.KeepAlive = 180;
                o.EndPoints.Add("localhost", 32768);
            });
            services.AddRedisClientFactory();
        }
        
        static void AddRabbitMqEventBus(IServiceCollection services,IConfiguration configuration)
        {
            services.AddSingleton<IntegrationEventHandlerActivator>();
            // 注入rabbitmq链接
            services.AddSingleton<IRabbitMQConnection>(sp =>
            {
                var factory = new ConnectionFactory()
                {
                    HostName = "localhost",
                    DispatchConsumersAsync = true
                };
                factory.UserName = "admin";
                factory.Password = "admin";
                return new DefaultRabbitMQConnection(factory);
            });
            
            // 注入rabbitmq事件总线
            services.AddSingleton<EventBusRabbitMQ>(sp =>
            {
                var connection = sp.GetRequiredService<IRabbitMQConnection>();
                var msa = sp.GetRequiredService<IntegrationEventHandlerActivator>();
                var declre = new RabbitMQDeclaration
                {
                    ExchangeName = "chat_app_exchange",
                    QueueName = $"user_message_queue_{EnvironmentConstants.EnvHostName}",
                    ConfigExchangeType = ExchangeType.Fanout
                };
                var logger = sp.GetRequiredService<ILogger<EventBusRabbitMQ>>();
                return new EventBusRabbitMQ(connection, declre, msa,logger:logger);
            });
        }
        
        public static void StartBasicConsume(this IServiceProvider appService)
        {
            var bus = appService.GetRequiredService<EventBusRabbitMQ>();
            bus.StartBasicConsume();
        }
    }
}