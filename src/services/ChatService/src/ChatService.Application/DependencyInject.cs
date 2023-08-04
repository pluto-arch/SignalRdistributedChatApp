using Dncy.Permission;
using ChatService.Application.Behaviors;
using ChatService.Application.Permission;
using ChatService.Infra.EntityFrameworkCore.Repository;
using ChatService.Application.IntegrationEvents;

namespace ChatService.Application
{
    public static class DependencyInject
    {
        public static IServiceCollection AddApplicationModule(this IServiceCollection services, IConfiguration _)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => !string.IsNullOrEmpty(x.FullName) && ( !x.FullName.Contains("Microsoft", StringComparison.OrdinalIgnoreCase) || !x.FullName.Contains("System", StringComparison.OrdinalIgnoreCase) ));
            services.AddAutoMapper(assemblies.ToArray());

            services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TransactionBehavior<,>).Assembly));

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));


            #region permission
            services.AddScoped<IPermissionChecker, PermissionChecker>();
            // permission definition 
            services.AddSingleton<IPermissionDefinitionManager, DefaultPermissionDefinitionManager>();
            services.AddSingleton<IPermissionDefinitionProvider, PermissionDefinitionProvider>();

            services.AddTransient<IPermissionGrantStore, EfCorePermissionGrantStore>();
            services.AddTransient<IPermissionManager, CachedPermissionManager>();
            services.AddTransient<IPermissionValueProvider, RolePermissionValueProvider>();
            services.AddTransient<IPermissionValueProvider, UserPermissionValueProvider>();
            #endregion


            services.AutoInjectChatService_Application();
            return services;
        }

    }
}