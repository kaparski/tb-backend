using TaxBeacon.Common.Models;

namespace TaxBeacon.Accounts.Contacts.Activities.Models;

public sealed class ContactLinkedToContactEvent: EventBase
{
    public DateTime LinkDate { get; init; }

    public Guid RelatedContactId { get; init; }

    public string RelatedContactName { get; init; }

    public ContactLinkedToContactEvent(Guid executorId,
        string executorRoles,
        string executorFullName,
        DateTime linkDate,
        Guid relatedContactId,
        string relatedContactName): base(executorId, executorRoles, executorFullName)
    {
        LinkDate = linkDate;
        RelatedContactId = relatedContactId;
        RelatedContactName = relatedContactName;
    }

    public override string ToString() => $"Contact linked with the contact: {RelatedContactName}";
}
