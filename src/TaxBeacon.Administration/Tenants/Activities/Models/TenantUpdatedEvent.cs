using TaxBeacon.Common.Models;

namespace TaxBeacon.Administration.Tenants.Activities.Models;

public sealed class TenantUpdatedEvent: EventBase
{
    public TenantUpdatedEvent(Guid executorId, string executorRoles, string executorFullName, DateTime updatedDate,
        string previousValues, string currentValues)
        : base(executorId, executorRoles, executorFullName)
    {
        UpdatedDate = updatedDate;
        PreviousValues = previousValues;
        CurrentValues = currentValues;
    }

    public string CurrentValues { get; set; }

    public string PreviousValues { get; set; }

    public DateTime UpdatedDate { get; set; }

    public override string ToString() => "Tenant details updated";
}
