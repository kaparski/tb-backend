using Microsoft.AspNetCore.Mvc;

namespace TaxBeacon.API.Controllers
{
    public class HomeController : ControllerBase
    {
        public IActionResult Index() => Ok();
    }
}
