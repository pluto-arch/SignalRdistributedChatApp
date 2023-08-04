using ChatService.Infra.EntityFrameworkCore.Repository;

namespace ChatService.Infra.EntityFrameworkCore
{
    internal sealed class EfRepositoryHelper
    {
        internal static Type GetRepositoryType(Type dbContextType, Type entityType)
        {
            return typeof(EfRepository<,>).MakeGenericType(dbContextType, entityType);
        }
        internal static Type GetRepositoryType(Type dbContextType, Type entityType, Type primaryKeyType)
        {
            return typeof(EfRepository<,,>).MakeGenericType(dbContextType, entityType, primaryKeyType);
        }
    }
}