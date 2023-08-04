﻿using Dncy.Permission;
using ChatService.Domain.Aggregates.System;
using ChatService.Domain.Infra.Repository;
using ChatService.Infra.EntityFrameworkCore.DbContexts;
using ChatService.Uow;

namespace ChatService.Infra.EntityFrameworkCore.Repository
{
    public class EfCorePermissionGrantStore : IPermissionGrantStore
    {
        private readonly IEfRepository<PermissionGrant> _permissionGrants;

        public EfCorePermissionGrantStore(IUnitOfWork<ChatServiceDbContext> uow)
        {
            _permissionGrants = uow.GetEfRepository<PermissionGrant>();
        }
        public async Task GrantAsync(string name, string providerName, string providerKey)
        {
            await _permissionGrants.InsertAsync(new PermissionGrant(name, providerName, providerKey), true);
        }

        public async Task GrantAsync(string[] name, string providerName, string providerKey)
        {
            var list = name.Select(x => new PermissionGrant(x, providerName, providerKey));
            await _permissionGrants.InsertAsync(list, true);
        }

        public async Task CancleGrantAsync(string name, string providerName, string providerKey)
        {
            await _permissionGrants.DeleteAsync(x => x.Name == name && x.ProviderKey == providerKey && x.ProviderName == providerName, true);
        }

        public async Task CancleGrantAsync(string[] name, string providerName, string providerKey)
        {
            await _permissionGrants.DeleteAsync(x => name.Contains(x.Name) && x.ProviderKey == providerKey && x.ProviderName == providerName, true);
        }

        public async Task<IPermissionGrant> GetAsync(string name, string providerName, string providerKey)
        {
            return await _permissionGrants.FirstOrDefaultAsync(x => x.Name == name && x.ProviderKey == providerKey && x.ProviderName == providerName);
        }

        public async Task<IEnumerable<IPermissionGrant>> GetListAsync(string providerName, string providerKey)
        {
            return await _permissionGrants.Where(x => x.ProviderKey == providerKey && x.ProviderName == providerName).ToListAsync();
        }

        public async Task<IEnumerable<IPermissionGrant>> GetListAsync(string[] names, string providerName, string providerKey)
        {
            return await _permissionGrants.Where(x => names.Contains(x.Name) && x.ProviderKey == providerKey && x.ProviderName == providerName).ToListAsync();
        }
    }
}
