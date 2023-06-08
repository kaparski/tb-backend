﻿using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL.Entities.Accounts;

namespace TaxBeacon.DAL.Interfaces;

public interface IAccountDbContext
{
    DbSet<Account> Accounts { get; }

    DbSet<Client> Clients { get; }

    DbSet<Referral> Referrals { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
