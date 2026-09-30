using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Couchbase.Extensions.DependencyInjection;

// The two HomeController variants below are illustrative alternatives shown in the docs.
// Each lives in its own child namespace so both can keep the name `HomeController`.
namespace Couchbase.Docs.Examples.Howtos.DependencyInjection.HomeControllerNamed
{
    // tag::namedbucketprovider[]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly INamedBucketProvider _provider;

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
    // end::namedbucketprovider[]
}

namespace Couchbase.Docs.Examples.Howtos.DependencyInjection.HomeControllerMyBucket
{
    // tag::IMybucketprovider[]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMyBucketProvider _provider;

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
    // end::IMybucketprovider[]
}
