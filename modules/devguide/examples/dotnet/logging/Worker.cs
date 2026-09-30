using System.Threading;
using System.Threading.Tasks;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Couchbase.Examples.Logging.GenericHost
{
    internal class Worker : IHostedService
    {
        private readonly ILogger<Worker> _logger;
        private readonly INamedBucketProvider _provider;

        public Worker(ILogger<Worker> logger, INamedBucketProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
           _logger.LogDebug($"Started bucket {_provider.BucketName}.");
           var bucket = await _provider.GetBucketAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Stopped.");

            var bucket = await _provider.GetBucketAsync();
            await bucket.DisposeAsync();
        }
    }
}