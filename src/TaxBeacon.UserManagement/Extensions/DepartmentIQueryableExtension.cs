using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.UserManagement.Extensions;

public static class DepartmentIQueryableExtension
{
    public static Task<DepartmentDetailsDto?> GetDepartmentDetailsAsync(this IQueryable<Department> query, Guid id, Guid tenantId) => query
        .Where(d => d.Id == id && d.TenantId == tenantId)
        .Select(d => new DepartmentDetailsDto
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            DivisionId = d.DivisionId,
            Division = d.Division == null ? string.Empty : d.Division.Name,
            ServiceAreas = d.ServiceAreas
                .Select(sa => new ServiceAreaDto
                {
                    Id = sa.Id,
                    Name = sa.Name,
                })
                .ToList()
        })
        .SingleOrDefaultAsync();
}
