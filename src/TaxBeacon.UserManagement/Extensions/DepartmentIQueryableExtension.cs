using TaxBeacon.DAL.Entities;
using TaxBeacon.UserManagement.Models;
using Microsoft.EntityFrameworkCore;

namespace TaxBeacon.UserManagement.Extensions;

public static class DepartmentIQueryableExtension
{
    public static Task<DepartmentDetailsDto> GetDepartmentDetails(this IQueryable<Department> query, Guid id) => query
        .Where(d => d.Id == id)
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
        .SingleAsync();
}
