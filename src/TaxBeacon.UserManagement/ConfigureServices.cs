using Microsoft.Extensions.DependencyInjection;
using TaxBeacon.UserManagement.Services;

namespace TaxBeacon.UserManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IUserService, UserService>();

        return serviceCollection;
    }
}
