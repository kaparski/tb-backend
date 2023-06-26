using Ardalis.SmartEnum;
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
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.Locations.Responses;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Extensions.GridifyServices;
using TaxBeacon.API.Extensions.SwaggerServices;
using TaxBeacon.API.Services;
using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Options;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.DAL.Interfaces;
using TaxBeacon.API.Controllers.Programs.Responses;

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
                .EnableQueryFeatures()
                .AddRouteComponents(
                    routePrefix: "api/odata",
                    model: GetODataEdmModel()
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/roles/{id}",
                    model: GetCustomODataEdmModel<RoleAssignedUserResponse>("RoleAssignedUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/departments/{id}",
                    model: GetCustomODataEdmModel<DepartmentUserResponse>("DepartmentUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/jobtitles/{id}",
                    model: GetCustomODataEdmModel<JobTitleUserResponse>("JobTitleUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/serviceareas/{id}",
                    model: GetCustomODataEdmModel<ServiceAreaUserResponse>("ServiceAreaUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/teams/{id}",
                    model: GetCustomODataEdmModel<TeamUserResponse>("TeamUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/divisions/{id}",
                    model: GetCustomODataEdmModel<DivisionUserResponse>("DivisionUsers")
                )
                .AddRouteComponents(
                    routePrefix: "api/accounts/{accountId}",
                    model: GetODataEdmModelForAccount()
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
            options.UseSqlServer(configuration.GetConnectionString("QA"),
                builder => builder.MigrationsAssembly(typeof(TaxBeaconDbContext).Assembly.FullName)));
        services.AddScoped<ITaxBeaconDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());
        services.AddScoped<IAccountDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());

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

        var clientState = modelBuilder.ComplexType<ClientState>();
        clientState.Property(c => c.Name);
        clientState.Property(c => c.Value);
        clientState.DerivesFrom<SmartEnum<ClientState>>();

        var referralState = modelBuilder.ComplexType<ReferralState>();
        referralState.Property(c => c.Name);
        referralState.Property(c => c.Value);
        referralState.DerivesFrom<SmartEnum<ReferralState>>();

        modelBuilder.EntitySet<AccountResponse>("Accounts");
        modelBuilder.EntitySet<DepartmentResponse>("Departments");
        modelBuilder.EntitySet<DivisionResponse>("Divisions");
        modelBuilder.EntitySet<TenantResponse>("Tenants");
        modelBuilder.EntitySet<JobTitleResponse>("JobTitles");
        modelBuilder.EntitySet<RoleResponse>("Roles");
        modelBuilder.EntitySet<ServiceAreaResponse>("ServiceAreas");
        modelBuilder.EntitySet<TeamResponse>("Teams");
        modelBuilder.EntitySet<ProgramResponse>("Programs");
        modelBuilder.EntitySet<ProgramResponse>("TenantPrograms");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }

    /// <summary>
    /// Builds a dedicated EDM model for an endpoint.
    /// </summary>
    private static IEdmModel GetCustomODataEdmModel<TEntity>(string entityName) where TEntity : class
    {
        var modelBuilder = new ODataConventionModelBuilder();

        modelBuilder.EntitySet<TEntity>(entityName);

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }

    /// <summary>
    /// Builds a dedicated EDM model for api/odata/accounts/{id} endpoint.
    /// </summary>
    private static IEdmModel GetODataEdmModelForAccount()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        modelBuilder.EntitySet<ContactResponse>("Contacts");
        modelBuilder.EntitySet<LocationResponse>("Locations");
        modelBuilder.EntitySet<EntityResponse>("Entities");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }
}
