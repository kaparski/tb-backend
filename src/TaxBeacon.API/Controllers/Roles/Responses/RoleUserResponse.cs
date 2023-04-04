﻿namespace TaxBeacon.API.Controllers.Roles.Responses
{
    public class RoleUserResponse
    {
        public Guid Id { get; set; }

        public string FullName { get; set; } = null!;

        public string Email { get; set; } = null!;
    }
}
