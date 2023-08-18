// Untested

using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace sdk_docs_dotnet_web_examples.Controllers
{
    {
    // #tag::IClusterProvider[]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IClusterProvider _provider;

        public HomeController(ILogger<HomeController> logger, IClusterProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var cluster= await _provider.GetClusterAsync();
            //do some work

            return View();
        }
    }
    // #end::IClusterProvider[]
    }
}

