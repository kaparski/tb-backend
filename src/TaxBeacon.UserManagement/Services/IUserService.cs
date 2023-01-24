﻿using Gridify;
using TaxBeacon.UserManagement.Models;
using System.Net.Mail;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task<QueryablePaging<UserList>> GetUsersAsync(GridifyQuery gridifyQuery, CancellationToken cancellationToken);
    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);
}
