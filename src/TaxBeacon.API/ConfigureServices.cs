using Ardalis.SmartEnum;
using FileSignatures;
using FluentValidation;
using Mapster;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OData;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Azure;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OData.Edm;
using Microsoft.OData.Json;
using Microsoft.OData.ModelBuilder;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using TaxBeacon.Accounts.Accounts.Models;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.Accounts.Responses;
using TaxBeacon.API.Controllers.ClientProspects.Responses;
using TaxBeacon.API.Controllers.Clients.Responses;
using TaxBeacon.API.Controllers.Contacts.Responses;
using TaxBeacon.API.Controllers.Departments.Responses;
using TaxBeacon.API.Controllers.Divisions.Responses;
using TaxBeacon.API.Controllers.Documents.Responses;
using TaxBeacon.API.Controllers.Entities.Responses;
using TaxBeacon.API.Controllers.JobTitles.Responses;
using TaxBeacon.API.Controllers.Locations.Responses;
using TaxBeacon.API.Controllers.Programs.Responses;
using TaxBeacon.API.Controllers.ReferralPartners.Responses;
using TaxBeacon.API.Controllers.ReferralProspects.Responses;
using TaxBeacon.API.Controllers.Roles.Responses;
using TaxBeacon.API.Controllers.ServiceAreas.Responses;
using TaxBeacon.API.Controllers.StateIds.Responses;
using TaxBeacon.API.Controllers.Teams.Responses;
using TaxBeacon.API.Controllers.Tenants.Responses;
using TaxBeacon.API.Controllers.Users.Responses;
using TaxBeacon.API.Extensions.Cors;
using TaxBeacon.API.Extensions.Swagger;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.API.Filters;
using TaxBeacon.API.Services;
using TaxBeacon.API.Shared.Validators;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.Enums.Accounts;
using TaxBeacon.Common.Options;
using TaxBeacon.Common.Services;
using TaxBeacon.DAL;
using TaxBeacon.DAL.Accounts;
using TaxBeacon.DAL.Administration;
using TaxBeacon.DAL.Interceptors;
using TaxBeacon.Email.Options;

namespace TaxBeacon.API;

public static class ConfigureServices
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddBlobServiceClient(configuration.GetSection("BlobServiceClient"));
            builder.AddSearchClient(configuration.GetSection("SearchClient"));
        });
        services.AddCorsService(configuration);

        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddControllers(options =>
            {
                options.Filters.Add<AuthorizeFilter>();
                options.Filters.Add<ValidationFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.Converters.Add(new SmartEnumConverterFactory());
            })
            // Configuring OData. Routing to OData endpoints is separate from normal Web API routing
            .AddOData(options => options
                .EnableQueryFeatures()
                .AddRouteComponents(
                    routePrefix: "api/odata",
                    model: GetODataEdmModel(),
                    s => s.AddSingleton<IStreamBasedJsonWriterFactory>(_ =>
                        DefaultStreamBasedJsonWriterFactory.Default)
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
                .AddRouteComponents(
                    routePrefix: "api/odata/entities/{entityId}",
                    model: GetODataEdmModelForEntities()
                )
                .AddRouteComponents(
                    routePrefix: "api/accounts",
                    model: GetODataEdmModelForAccounts()
                )
                // TODO: find a way to use attribute routing
                .AddRouteComponents(
                    routePrefix: "api/odata/accounts/{accountId}",
                    model: GetCustomODataEdmModel<AccountContactResponse>("AccountContacts")
                )
                .AddRouteComponents(
                    routePrefix: "api/odata/contacts/{contactId:guid}",
                    model: GetCustomODataEdmModel<LinkedContactDetailsResponse>("LinkedContacts")
                )
            );

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwagger(configuration);
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly(),
            filter: filter => filter.ValidatorType != typeof(FormFileValidator));
        services.Configure<AzureAd>(configuration.GetSection(nameof(AzureAd)));
        services.Configure<SendGridOptions>(configuration.GetSection(SendGridOptions.SendGrid));
        services.Configure<CreateUserOptions>(configuration.GetSection(CreateUserOptions.CreateUser));
        services.Configure<LoadTestingOptions>(configuration.GetSection(LoadTestingOptions.LoadTesting));

        services.AddScoped<EntitySaveChangesInterceptor>();
        services.AddDbContext<TaxBeaconDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                builder => builder.MigrationsAssembly(typeof(TaxBeaconDbContext).Assembly.FullName)));
        services.AddScoped<ITaxBeaconDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());
        services.AddScoped<IAccountDbContext>(provider => provider.GetRequiredService<TaxBeaconDbContext>());

        var loadTestingOptions = configuration.GetSection(LoadTestingOptions.LoadTesting)
                                              .Get<LoadTestingOptions>();

        if (loadTestingOptions?.IsLoadTestingEnabled != true)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));
        }
        else
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                    .AddJwtBearer(Schemas.LoadTestingSchema, options
                    => options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = loadTestingOptions.Issuer,
                        ValidAudience = loadTestingOptions.Audience,
                        IssuerSigningKey = new SymmetricSecurityKey(new HMACSHA256(Encoding.UTF8.GetBytes(loadTestingOptions.Key)).Key)
                    })
                    .AddMicrosoftIdentityWebApi(configuration.GetSection("AzureAd"));

            services.AddAuthorization(options =>
            {
                var defaultAuthorizationPolicyBuilder = new AuthorizationPolicyBuilder(
                    JwtBearerDefaults.AuthenticationScheme,
                    Schemas.LoadTestingSchema);
                defaultAuthorizationPolicyBuilder =
                    defaultAuthorizationPolicyBuilder.RequireAuthenticatedUser();
                options.DefaultPolicy = defaultAuthorizationPolicyBuilder.Build();
            });
        }

        services.AddTransient<IClaimsTransformation, ClaimsTransformation>();
        services.AddSingleton<IAuthorizationHandler, PermissionsAuthorizationHandler>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionsAuthorizationPolicyProvider>();
        services.AddSingleton<IFileFormatInspector>(new FileFormatInspector());

        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ICurrentTimeZoneService, CurrentTimeZoneService>();
        services.AddScoped<IGlobalSearchService, GlobalSearchService>();

        services.AddFeatureManagement(configuration);

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
        modelBuilder.EntitySet<TenantProgramResponse>("TenantPrograms");
        modelBuilder.EntitySet<ContactResponse>("Contacts");

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

        modelBuilder.EntitySet<LocationResponse>("Locations");
        modelBuilder.EntitySet<DocumentResponse>("Documents");
        var entitySet = modelBuilder.EntitySet<EntityResponse>("Entities");

        modelBuilder.EntityType<EntityLocationResponse>()
            .HasKey(x => new { x.EntityId, x.LocationId });

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }

    /// <summary>
    /// Builds a dedicated EDM model for api/odata/entities/{id} endpoint.
    /// </summary>
    private static IEdmModel GetODataEdmModelForEntities()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        var clientState = modelBuilder.ComplexType<StateIdType>();
        clientState.Property(c => c.Name);
        clientState.Property(c => c.Value);
        clientState.DerivesFrom<SmartEnum<StateIdType>>();

        modelBuilder.EntityType<StateIdResponse>().HasKey(c => new { c.StateIdCode, c.TenantId });
        modelBuilder.EntitySet<StateIdResponse>("StateIds");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }

    /// <summary>
    /// Builds a dedicated EDM model for api/odata/accounts/ endpoint.
    /// </summary>
    private static IEdmModel GetODataEdmModelForAccounts()
    {
        var modelBuilder = new ODataConventionModelBuilder();

        var salesperson = modelBuilder.ComplexType<SalespersonDto>();
        salesperson.Property(c => c.Id);
        salesperson.Property(c => c.FullName);

        modelBuilder.EntitySet<ClientProspectResponse>("ClientProspects");
        modelBuilder.EntitySet<ClientResponse>("Clients");
        modelBuilder.EntitySet<ReferralProspectResponse>("ReferralProspects");
        modelBuilder.EntitySet<ReferralPartnerResponse>("ReferralPartners");

        modelBuilder.EnableLowerCamelCase();

        return modelBuilder.GetEdmModel();
    }
}
