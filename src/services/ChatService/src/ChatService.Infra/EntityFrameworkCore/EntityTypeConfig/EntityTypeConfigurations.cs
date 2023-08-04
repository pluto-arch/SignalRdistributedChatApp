using ChatService.Domain.Aggregates.System;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatService.Infra.EntityFrameworkCore.EntityTypeConfig;


public class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<PermissionGrant>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<PermissionGrant> builder)
    {
        builder.ToTable("PermissionGrant");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Name).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProviderName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ProviderKey).HasMaxLength(300).IsRequired();
    }
}
