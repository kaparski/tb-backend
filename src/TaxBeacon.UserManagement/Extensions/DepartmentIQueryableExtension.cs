using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models;

namespace TaxBeacon.UserManagement.Extensions;

public static class DepartmentIQueryableExtension
{
    public static IQueryable<DepartmentDto> ToDepartmentDto(this IQueryable<Department> query) => query
        .Select(d => new DepartmentDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            AssignedUsersCount = d.Users.Count(),
            DivisionId = d.DivisionId,
            Division = d.Division == null ? string.Empty : d.Division.Name,
            ServiceAreas = string.Join(", ", d.ServiceAreas.Select(sa => sa.Name)),
            ServiceArea = d.ServiceAreas.Select(sa => sa.Name)
                    .GroupBy(sa => 1)
                    .Select(g => string.Join(string.Empty, g.Select(s => "|" + s + "|")))
                    .FirstOrDefault() ?? string.Empty
        });
}
