using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaxBeacon.Common.Permissions;
using TaxBeacon.DAL.Accounts.Entities;

namespace TaxBeacon.DAL.Accounts.Configurations;
public class ClientViewConfiguration: IEntityTypeConfiguration<ClientView>
{
    public void Configure(EntityTypeBuilder<ClientView> clientView)
    {
        clientView.HasKey(x => new
        {
            x.TenantId,
            x.AccountId
        });

        clientView
            .Property(a => a.State)
            .HasColumnType("nvarchar")
            .HasMaxLength(2) // maybe it should be 4 for 'none'?
            .HasConversion<string>();

        clientView
            .Property(a => a.Status)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        clientView
            .Property(a => a.ClientState)
            .HasConversion<string>()
            .HasColumnType("nvarchar")
            .HasMaxLength(100);

        clientView
            .Property(c => c.AnnualRevenue)
            .HasColumnType("decimal")
            .HasPrecision(15, 2);

        clientView.ToTable("ClientsView", t => t.ExcludeFromMigrations());
    }
}
