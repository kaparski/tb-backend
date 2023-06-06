using FluentValidation;
using FluentValidation.AspNetCore;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Reflection;
using System.Text.Json.Serialization;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Extensions.GridifyServices;
using TaxBeacon.API.Extensions.SwaggerServices;
using TaxBeacon.API.Services;
using TaxBeacon.Common.Options;
using TaxBeacon.Common.Services;
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
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers(/*options => options.Filters.Add<AuthorizeFilter>()*/)
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()))
        // Configuring OData. Routing to OData endpoints is separate from normal Web API routing
            .AddOData(options => options
                .EnableQueryFeatures(null)
                .AddRouteComponents(
                    routePrefix: "api/odata",
                    model: GetODataEdmModel()
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/roles/{id}",
                    model: GetODataEdmModelForRoleAssignedUsers()
                )
            );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwagger();
        services.AddGridify(configuration);
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.Configure<AzureAd>(configuration.GetSection(nameof(AzureAd)));

        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddDbContext<TaxBeaconDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(TaxBeaconDbContext).Assembly.FullName)));
        services.AddScoped<ITaxBeaconDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

        services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        services.AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionsAuthorizationPolicyProvider>();

        services.AddCors(o => o.AddPolicy("DefaultCorsPolicy", builder =>
            builder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .WithExposedHeaders("Content-Disposition")));

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTimeZoneService, CurrentTimeZoneService>();
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());

        return services;
    }

    /// <summary>
    /// Builds EDM model for OData endpoints (this model needs to be different from EF model, since exposed entities are different)
    /// </summary>
    private static IEdmModel GetODataEdmModel()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        // EntitySet name here should match controller's name
        modelBuilder.EntitySet<UserResponse>("Users");
        modelBuilder.EntitySet<DepartmentResponse>("Departments");
        modelBuilder.EntitySet<DivisionResponse>("Divisions");
        modelBuilder.EntitySet<TenantResponse>("Tenants");
        modelBuilder.EntitySet<JobTitleResponse>("JobTitles");
        modelBuilder.EntitySet<RoleResponse>("Roles");
        modelBuilder.EntitySet<ServiceAreaResponse>("ServiceAreas");
        modelBuilder.EntitySet<TeamResponse>("Teams");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }

    /// <summary>
    /// Builds a dedicated EDM model for api/odata/roles/{id:guid}/roleassignedusers endpoint.
    /// TODO: find a better way to make this custom routing work.
    /// </summary>
    /// <returns></returns>
    private static IEdmModel GetODataEdmModelForRoleAssignedUsers()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        modelBuilder.EntitySet<RoleAssignedUserResponse>("RoleAssignedUsers");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }
}
