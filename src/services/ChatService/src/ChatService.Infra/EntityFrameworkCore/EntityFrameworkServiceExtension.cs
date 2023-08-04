using Dncy.Specifications.EntityFrameworkCore;
using ChatService.Domain.Infra;
using ChatService.Domain.Infra.Repository;
using ChatService.Infra.Constants;
using ChatService.Infra.EntityFrameworkCore.ConnectionStringResolve;
using ChatService.Infra.EntityFrameworkCore.DbContexts;
using ChatService.Infra.EntityFrameworkCore.Interceptor;
using ChatService.Uow;
using ChatService.Uow.EntityFrameworkCore;

namespace ChatService.Infra.EntityFrameworkCore;

public static class EntityFrameworkServiceExtension
{

    /// <summary>
    /// 添加efcore 组件
    /// </summary>
    /// <returns></returns>
    internal static IServiceCollection AddEfCoreInfraComponent(this IServiceCollection service, IConfiguration configuration, List<Type> contextTypes)
    {
        service.AddSingleton<IConnectionStringResolve, DefaultConnectionStringResolve>();
        service.AddDbContextPool<ChatServiceDbContext>((serviceProvider, optionsBuilder) =>
        {
            optionsBuilder.UseSqlServer(configuration.GetConnectionString(DbConstants.DEFAULT_CONNECTIONSTRING_NAME),
                sqlOptions =>
                {
                    sqlOptions.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                    //sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromMilliseconds(400), null);
                });

            var mediator = serviceProvider.GetService<IDomainEventDispatcher>() ?? NullDomainEventDispatcher.Instance;
            optionsBuilder.AddInterceptors(new DataChangeSaveChangesInterceptor(mediator));



        });

        service.AddDefaultRepository(contextTypes);
        service.ApplyEntityDefaultNavicationProperty();
        return service;
    }


    public static void AddEfUnitofWork(this IServiceCollection services, List<Type> context = null)
    {
        if (context is null or { Count: <= 0 })
        {
            return;
        }

        if (context.Count == 1)
        {
            var defType = typeof(IUnitOfWork);
            var defType2 = typeof(EfUnitOfWork<>).MakeGenericType(context[0]);
            services.RegisterType(defType, defType2);
        }

        Parallel.ForEach(context, item =>
        {
            var defType = typeof(IUnitOfWork<>).MakeGenericType(item);
            var defType2 = typeof(EfUnitOfWork<>).MakeGenericType(item);
            services.RegisterType(defType, defType2);
        });
    }


    #region private

    private static void ApplyEntityDefaultNavicationProperty(this IServiceCollection service)
    {
        // 设置实体默认显示加载的导航属性
        service.Configure<IncludeRelatedPropertiesOptions>(options =>
        {
           
        });
    }



    private static void AddDefaultRepository(this IServiceCollection services, List<Type> context = null)
    {
        if (context is null or { Count: <= 0 })
        {
            return;
        }

        Parallel.ForEach(context, item =>
        {
            var entitTypies =
                from property in item.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where IsAssignableToGenericType(property.PropertyType, typeof(DbSet<>)) &&
                      typeof(IEntity).IsAssignableFrom(property.PropertyType.GenericTypeArguments[0])
                select property.PropertyType.GenericTypeArguments[0];


            foreach (var entityType in entitTypies)
            {
                var defType = typeof(IEfRepository<>).MakeGenericType(entityType);
                var defType2 = typeof(IEfContextRepository<,>).MakeGenericType(item, entityType);
                var implementingType = EfRepositoryHelper.GetRepositoryType(item, entityType);
                services.RegisterType(defType, implementingType);
                services.RegisterType(defType2, implementingType);

                Type keyType = EntityHelper.FindPrimaryKeyType(entityType);
                if (keyType != null)
                {
                    var impl = EfRepositoryHelper.GetRepositoryType(item, entityType, keyType);
                    services.RegisterType(typeof(IEfRepository<,>).MakeGenericType(entityType, keyType), impl);
                    services.RegisterType(typeof(IEfContextRepository<,,>).MakeGenericType(item, entityType, keyType),
                        impl);
                }
            }
        });
    }

    private static IServiceCollection RegisterType(this IServiceCollection services, Type type, Type implementationType)
    {
        if (type.IsAssignableFrom(implementationType))
        {
            services.TryAddTransient(type, implementationType);
        }
        return services;
    }


    public static bool IsAssignableToGenericType(Type givenType, Type genericType)
    {
        TypeInfo typeInfo = givenType.GetTypeInfo();
        if (typeInfo.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
        {
            return true;
        }

        Type[] interfaces = typeInfo.GetInterfaces();
        foreach (Type type in interfaces)
        {
            if (type.GetTypeInfo().IsGenericType && type.GetGenericTypeDefinition() == genericType)
            {
                return true;
            }
        }

        if (typeInfo.BaseType == null)
        {
            return false;
        }

        return IsAssignableToGenericType(typeInfo.BaseType, genericType);
    }
    #endregion
}