using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ReferralPartners.Requests;
using TaxBeacon.API.Controllers.ReferralPartners.Responses;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.FeatureManagement;
using TaxBeacon.Common.Enums;
using TaxBeacon.Common.Converters;

namespace TaxBeacon.API.Controllers.ReferralPartners;

[Authorize]
[FeatureGate(FeatureFlagKeys.Referrals)]
public class ReferralPartnersController: BaseController
{
    private readonly IAccountService _accountService;
    public ReferralPartnersController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Get referral partners
    /// </summary>
    /// <response code="200">Returns referral partners</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Get referrls partners</returns>
    [HasPermissions(Common.Permissions.Referrals.Read, Common.Permissions.Accounts.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/referralpartners", Name = "GetReferralPartners")]
    [ProducesResponseType(typeof(IQueryable<ReferralPartnerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetReferralPartners()
    {
        var referralPartners = _accountService.QueryReferralPartners();
        return Ok(referralPartners.ProjectToType<ReferralPartnerResponse>());
    }

    /// <summary>
    /// Export referral partners
    /// </summary>
    /// <response code="200">Export referral partners</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Export referral partners</returns>
    [HasPermissions(Common.Permissions.Referrals.ReadExport, Common.Permissions.Accounts.ReadExport)]
    [HttpGet("/api/accounts/referralpartners/export", Name = "ExportReferralPartners")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportReferralPartnersAsync([FromQuery] ExportReferralPartnersRequest request,
        CancellationToken cancellationToken)
    {
        var mimeType = request.FileType.ToMimeType();

        var referralPartners = await _accountService.ExportReferralPartnersAsync(
            request.FileType, cancellationToken);

        return File(referralPartners, mimeType,
            $"referral-partners.{request.FileType.ToString().ToLowerInvariant()}");
    }
}
