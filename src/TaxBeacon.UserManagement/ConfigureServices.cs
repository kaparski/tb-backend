using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.UserManagement.Services;
using TaxBeacon.UserManagement.Services.Activities;
using TaxBeacon.UserManagement.Services.Activities.Department;
using TaxBeacon.UserManagement.Services.Activities.JobTitle;
using TaxBeacon.UserManagement.Services.Activities.ServiceArea;
using TaxBeacon.UserManagement.Services.Contacts;
using TaxBeacon.UserManagement.Services.Program;
using TaxBeacon.UserManagement.Services.Program.Activities;
using TaxBeacon.UserManagement.Services.Tenants;
using TaxBeacon.UserManagement.Services.Tenants.Activities;

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
        serviceCollection.AddScoped<IDivisionsService, DivisionsService>();
        serviceCollection.AddScoped<ITableFiltersService, TableFilterService>();
        serviceCollection.AddScoped<IUserActivityFactory, UserCreatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, AssignRolesEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserDeactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserReactivatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UserUpdatedEventFactory>();
        serviceCollection.AddScoped<IUserActivityFactory, UnassignRolesEventFactory>();

        serviceCollection.AddScoped<ITeamService, TeamService>();
        serviceCollection.AddScoped<IDepartmentService, DepartmentService>();
        serviceCollection.AddScoped<IServiceAreaService, ServiceAreaService>();
        serviceCollection.AddScoped<IProgramService, ProgramService>();
        serviceCollection.AddScoped<IJobTitleService, JobTitleService>();
        serviceCollection.AddScoped<IContactService, ContactService>();

        serviceCollection.AddScoped<ITeamActivityFactory, TeamUpdatedEventFactory>();

        serviceCollection.AddScoped<ITenantActivityFactory, TenantEnteredEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantExitedEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantUpdatedEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantAssignProgramsEventFactory>();
        serviceCollection.AddScoped<ITenantActivityFactory, TenantUnassignProgramsEventFactory>();

        serviceCollection.AddScoped<IDivisionActivityFactory, DivisionUpdatedEventFactory>();

        serviceCollection.AddScoped<IDepartmentActivityFactory, DepartmentUpdatedEventFactory>();

        serviceCollection.AddScoped<IServiceAreaActivityFactory, ServiceAreaUpdatedEventFactory>();

        serviceCollection.AddScoped<IJobTitleActivityFactory, JobTitleUpdatedEventFactory>();

        serviceCollection.AddScoped<IProgramActivityFactory, ProgramCreatedEventFactory>();
        serviceCollection.AddScoped<IProgramActivityFactory, ProgramDeactivatedEventFactory>();
        serviceCollection.AddScoped<IProgramActivityFactory, ProgramReactivatedEventFactory>();
        serviceCollection.AddScoped<IProgramActivityFactory, ProgramUpdatedEventFactory>();
        serviceCollection.AddScoped<IProgramActivityFactory, ProgramOrgUnitUnassignEventFactory>();
        serviceCollection.AddScoped<IProgramActivityFactory, ProgramOrgUnitAssignEventFactory>();

        return serviceCollection;
    }
}
