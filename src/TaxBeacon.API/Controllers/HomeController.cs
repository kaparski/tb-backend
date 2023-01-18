using Microsoft.AspNetCore.Mvc;

namespace TaxBeacon.API.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return Ok();
        }
    }
}
