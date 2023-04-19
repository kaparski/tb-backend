﻿namespace TaxBeacon.API.Controllers.Tenants.Responses
{
    public class DivisionResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int NumberOfUsers { get; set; }
        public string Description { get; set; }

        public DateTime CreatedDateTimeUtc { get; set; }

        public string Departments { get; set; }
    }
}
