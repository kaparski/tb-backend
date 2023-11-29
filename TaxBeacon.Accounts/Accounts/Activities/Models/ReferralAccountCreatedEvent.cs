using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Accounts.Activities.Models;

public sealed class ReferralAccountCreatedEvent: EventBase
{
    public DateTime CreatedDate { get; }

    public ReferralAccountCreatedEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime createdDate
        ) : base(executorId, executorRoles, executorFullName) =>
        CreatedDate = createdDate;


    public override string ToString() => "Referral account created";
}
