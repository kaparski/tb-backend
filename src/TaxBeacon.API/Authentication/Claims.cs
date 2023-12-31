﻿namespace TaxBeacon.API.Authentication;

public static class Claims
{
    public const string UserId = "userId";
    public const string EmailClaimName = "preferred_username";
    public const string TenantId = "tenantId";
    public const string OtherMails = "otherMails";
    public const string Roles = "roles";
    public const string TenantRoles = "tenantRoles";
    public const string FullName = "fullName";
    public const string DivisionEnabled = "divisionEnabled";
    public const string Permission = "permission";
}