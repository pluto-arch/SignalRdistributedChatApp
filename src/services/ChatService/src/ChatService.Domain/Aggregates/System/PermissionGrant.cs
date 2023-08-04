using Dncy.Permission;

using ChatService.Domain.Infra;

namespace ChatService.Domain.Aggregates.System;

public class PermissionGrant : BaseEntity<int>, IPermissionGrant
{

    public PermissionGrant()
    {
        CreateTime = DateTimeOffset.Now;
    }

    public PermissionGrant(string name, string providerName, string providerKey) : this()
    {
        Name = name;
        ProviderName = providerName;
        ProviderKey = providerKey;
    }

    /// <inheritdoc />
    public string Name { get; set; }

    /// <inheritdoc />
    public string ProviderName { get; set; }

    /// <inheritdoc />
    public string ProviderKey { get; set; }


    public DateTimeOffset CreateTime { get; set; }
}