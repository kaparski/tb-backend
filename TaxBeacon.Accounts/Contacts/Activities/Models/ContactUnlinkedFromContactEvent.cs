using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;
public sealed class ContactUnlinkedFromContactEvent: EventBase
{
    public DateTime UnlinkDate { get; init; }

    public Guid RelatedContactId { get; init; }

    public string ContactName { get; init; }

    public ContactUnlinkedFromContactEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime unlinkDate,
        Guid relatedContactId,
        string contactName) : base(executorId, executorRoles, executorFullName)
    {
        UnlinkDate = unlinkDate;
        RelatedContactId = relatedContactId;
        ContactName = contactName;
    }

    public override string ToString() => $"Contact unlinked from the contact: {ContactName}";
}
