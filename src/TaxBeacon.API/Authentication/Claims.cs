namespace TaxBeacon.API.Authentication
{
    public static class Claims
    {
        public const string UserIdClaimName = "userId";
        public const string EmailClaimName = "preferred_username";
        public const string TenantId = "tenantId";
        public const string OtherMails = "otherMails";
        public const string Roles = "roles";
        public const string TenantRoles = "tenantRoles";
        public const string FullName = "fullName";
    }
}
