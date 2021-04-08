using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Couchbase.Net.DevGuide
{
    /// <summary>
    /// For an example of configuring the Couchbase connection through App.config/Web.config
    /// see the ConnectionConfig class
    /// </summary>
    public class ConnectionBase : IAsyncDisposable
    {
        protected ICluster Cluster;
        protected IBucket Bucket;

        protected async Task ConnectAsync()
        {
            await ConnectAsync("default");
        }

        protected async Task ConnectAsync(String bucketName)
        {
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddFilter(level => level >= LogLevel.Debug)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            loggerFactory.AddFile("Logs/myapp-{Date}.txt", LogLevel.Debug);

            Logger = loggerFactory.CreateLogger("examples");

            var options = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("couchbase")
                .Get<ClusterOptions>()
                .WithLogging(loggerFactory);

            Cluster = await Couchbase.Cluster.ConnectAsync(options).ConfigureAwait(false);
            Bucket = await Cluster.BucketAsync(bucketName).ConfigureAwait(false);
        }

        public virtual async Task ExecuteAsync()
        {
            await ConnectAsync().ConfigureAwait(false);
            Console.WriteLine("Connected to bucket '{0}'", Bucket.Name);
        }

        public static async Task Main(string[] args)
        {
           await new ConnectionBase().ExecuteAsync().ConfigureAwait(false);
        }


        public async ValueTask DisposeAsync()
        {
            await Bucket.DisposeAsync().ConfigureAwait(false);
            await Cluster.DisposeAsync().ConfigureAwait(false);
        }

        public ILogger Logger { get; set; }
    }
}
