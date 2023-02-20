﻿using Gridify;
using System.Net.Mail;
using TaxBeacon.Common.Enums;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Services;

public interface IUserService
{
    Task<QueryablePaging<UserDto>> GetUsersAsync(GridifyQuery gridifyQuery,
        CancellationToken cancellationToken = default);

    Task LoginAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserDto> GetUserByEmailAsync(MailAddress mailAddress, CancellationToken cancellationToken = default);

    Task<UserDto> UpdateUserStatusAsync(Guid id, UserStatus userStatus, CancellationToken cancellationToken = default);

    Task<UserDto> CreateUserAsync(
        UserDto user,
        CancellationToken cancellationToken = default);
}
