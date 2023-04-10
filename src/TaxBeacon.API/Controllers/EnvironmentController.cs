using Microsoft.AspNetCore.Mvc;

namespace TaxBeacon.API.Controllers
{
    public class EnvironmentController: BaseController
    {
        private readonly IConfiguration _configuration;

        public EnvironmentController(IConfiguration configuration) => _configuration = configuration;

        [HttpGet]
        public IActionResult Index() => Ok(_configuration["ConnectionStrings__DefaultConnection"]);
    }
}
