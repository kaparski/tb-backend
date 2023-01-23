﻿using Gridify;
using TaxBeacon.UserManagement.Models;
using System.Net.Mail;


namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    public QueryablePaging<UserList> GetUsers(GridifyQuery gridifyQuery);
    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);
}
