using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.UserManagement.Models.Activities;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities;

namespace TaxBeacon.UserManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<ITenantService, TenantService>();
        serviceCollection.AddScoped<IRoleService, RoleService>();
        serviceCollection.AddScoped<IPasswordGenerator, PasswordGenerator>();
        serviceCollection.AddScoped<IUserExternalStore, UserExternalStore>();
        serviceCollection.AddScoped<ITenantDivisionsService, TenantDivisionsService>();
        serviceCollection.AddScoped<IPermissionsService, PermissionsService>();
        serviceCollection.AddScoped<ITableFiltersService, TableFilterService>();
        serviceCollection.AddScoped<IUserActivityFactory, UserCreatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, AssignRolesEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserDeactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserReactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserUpdatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UnassignRolesEventFactory>();
        return serviceCollection;
    }
}
