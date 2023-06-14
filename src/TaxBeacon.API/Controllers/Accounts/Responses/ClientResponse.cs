using TaxBeacon.Common.Accounts;
using TaxBeacon.Common.Enums;

namespace TaxBeacon.API.Controllers.Accounts.Responses;

public record ClientResponse(ClientState State, Status Status);
