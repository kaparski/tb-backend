using TaxBeacon.API.Controllers.Departments.Responses;

namespace TaxBeacon.API.Controllers.Teams.Responses
{
    public class TeamDetailsResponse
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime CreatedDateTimeUtc { get; set; }
    }
}
