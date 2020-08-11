using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace sdk_docs_dotnet_web_examples.Controllers
{
    {
    // #tag::namedbucketprovider[]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly ITravelSampleBucketProvider _provider;

        public HomeController(ILogger<HomeController> logger, INamedBucketProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var bucket = await _provider.GetBucketAsync();
            //do some work

            return View();
        }
    }
    // #endtag::namedbucketprovider[]
    }
    {
        // #tag::IMybucketprovider[]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        public readonly ITravelSampleBucketProvider _provider;

        public HomeController(ILogger<HomeController> logger, IMyBucketProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task<IActionResult> IndexAsync()
        {
            var bucket = await _provider.GetBucketAsync();
            //do some work

            return View();
        }
    }
    // #endtag::IMybucketprovider[]
    }
}