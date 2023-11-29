using Mapster;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using TaxBeacon.Accounts.Accounts;
using TaxBeacon.API.Authentication;
using TaxBeacon.API.Controllers.ReferralProspects.Requests;
using TaxBeacon.API.Controllers.ReferralProspects.Responses;
using TaxBeacon.API.FeatureManagement;
using TaxBeacon.Common.Converters;
using TaxBeacon.Common.FeatureManagement;

namespace TaxBeacon.API.Controllers.ReferralProspects;

[Authorize]
[FeatureGate(FeatureFlagKeys.Referrals)]
public class ReferralProspectsController: BaseController
{
    private readonly IAccountService _accountService;
    public ReferralProspectsController(IAccountService accountService) => _accountService = accountService;

    /// <summary>
    /// Get referral prospects
    /// </summary>
    /// <response code="200">Returns referral prospects</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Get referrls prospects</returns>
    [HasPermissions(Common.Permissions.Referrals.Read, Common.Permissions.Accounts.Read)]
    [EnableQuery]
    [HttpGet("api/accounts/referralprospects", Name = "GetReferralProspects")]
    [ProducesResponseType(typeof(IQueryable<ReferralProspectResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult GetReferralProspects()
    {
        var referralProspects = _accountService.QueryReferralsProspects();
        return Ok(referralProspects.ProjectToType<ReferralProspectResponse>());
    }

    /// <summary>
    /// Export referral prospects
    /// </summary>
    /// <response code="200">Export referral prospects</response>
    /// <response code="401">User is unauthorized</response>
    /// <response code="403">The user does not have the required permission</response>
    /// <returns>Export referral prospects</returns>
    [HasPermissions(Common.Permissions.Referrals.ReadExport, Common.Permissions.Accounts.ReadExport)]
    [HttpGet("/api/accounts/referralprospects/export", Name = "ExportReferralProspects")]
    [ProducesResponseType(typeof(byte[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> ExportReferralProspectsAsync([FromQuery] ExportReferralProspectsRequest request,
        CancellationToken cancellationToken)
    {
        var mimeType = request.FileType.ToMimeType();

        var referralProspects = await _accountService.ExportReferralProspectsAsync(
            request.FileType, cancellationToken);

        return File(referralProspects, mimeType,
            $"referral-prospects.{request.FileType.ToString().ToLowerInvariant()}");
    }
}
