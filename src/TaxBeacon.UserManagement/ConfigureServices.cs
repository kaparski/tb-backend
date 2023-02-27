﻿using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IUserService, UserService>();

        return serviceCollection;
    }
}
