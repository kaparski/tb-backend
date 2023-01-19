using Microsoft.EntityFrameworkCore;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;

namespace TaxBeacon.API;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Add services to the container.
        services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        // TODO: Decide if we should move this into TaxBeacon.UserManagement layer
        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddDbContext<TaxBeaconDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(TaxBeaconDbContext).Assembly.FullName)));
        services.AddScoped<ITaxBeaconDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());

        return services;
    }
}
