namespace TaxBeacon.Common.Enums.Accounts.Activities;

public enum AccountEventType
{
    None = 0,

    AccountCreated = 1,

    AccountProfileUpdated = 2,

    ClientDeactivated = 3,

    ClientReactivated = 4,

    ClientDetailsUpdated = 5,

    ClientAccountCreated = 6,

    ClientAccountManagerAssigned = 7,

    ClientAccountManagerUnassigned = 8,

    SalespersonAssigned = 9,

    SalespersonUnassigned = 10,

    EntitiesImportedSuccessfully = 11,

    EntitiesImportFailed = 12,

    ReferralAccountCreated = 13,

    ReferralAccountManagerAssigned = 14,

    ReferralAccountManagerUnassigned = 15,
}
