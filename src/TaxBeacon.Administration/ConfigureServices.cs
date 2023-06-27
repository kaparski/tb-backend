using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TaxBeacon.Administration.Departments;
using TaxBeacon.Administration.Departments.Activities.Factories;
using TaxBeacon.Administration.Divisions;
using TaxBeacon.Administration.Divisions.Activities.Factories;
using TaxBeacon.Administration.JobTitles;
using TaxBeacon.Administration.JobTitles.Activities.Factories;
using TaxBeacon.Administration.PasswordGenerator;
using TaxBeacon.Administration.Programs;
using TaxBeacon.Administration.Programs.Activities;
using TaxBeacon.Administration.Roles;
using TaxBeacon.Administration.ServiceAreas;
using TaxBeacon.Administration.ServiceAreas.Activities.Factories;
using TaxBeacon.Administration.TableFilters;
using TaxBeacon.Administration.Teams;
using TaxBeacon.Administration.Teams.Activities.Factories;
using TaxBeacon.Administration.Tenants;
using TaxBeacon.Administration.Tenants.Activities;
using TaxBeacon.Administration.Users;
using TaxBeacon.Administration.Users.Activities.Factories;

namespace TaxBeacon.Administration;

public static class ConfigureServices
{
    public static IServiceCollection AddAdministrationServices(this IServiceCollection serviceCollection)
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
