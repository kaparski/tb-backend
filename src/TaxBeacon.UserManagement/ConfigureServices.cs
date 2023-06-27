using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.UserManagement.Departments;
using TaxBeacon.UserManagement.Departments.Activities.Factories;
using TaxBeacon.UserManagement.Divisions;
using TaxBeacon.UserManagement.Divisions.Activities.Factories;
using TaxBeacon.UserManagement.JobTitles;
using TaxBeacon.UserManagement.JobTitles.Activities.Factories;
using TaxBeacon.UserManagement.PasswordGenerator;
using TaxBeacon.UserManagement.Programs;
using TaxBeacon.UserManagement.Programs.Activities;
using TaxBeacon.UserManagement.Roles;
using TaxBeacon.UserManagement.ServiceAreas;
using TaxBeacon.UserManagement.ServiceAreas.Activities.Factories;
using TaxBeacon.UserManagement.TableFilters;
using TaxBeacon.UserManagement.Teams;
using TaxBeacon.UserManagement.Teams.Activities.Factories;
using TaxBeacon.UserManagement.Tenants;
using TaxBeacon.UserManagement.Tenants.Activities;
using TaxBeacon.UserManagement.Users;
using TaxBeacon.UserManagement.Users.Activities.Factories;

namespace TaxBeacon.UserManagement;

public static class ConfigureServices
{
    public static IServiceCollection AddUserManagementServices(this IServiceCollection serviceCollection)
    {
        TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
        serviceCollection.AddScoped<IUserService, UserService>();
        serviceCollection.AddScoped<ITenantService, TenantService>();
        serviceCollection.AddScoped<IRoleService, RoleService>();
        serviceCollection.AddScoped<IPasswordGenerator, PasswordGenerator.PasswordGenerator>();
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
