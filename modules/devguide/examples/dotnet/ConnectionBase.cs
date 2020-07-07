using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

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
            var options = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .Build()
                .GetSection("couchbase")
                .Get<ClusterOptions>();

            Cluster = await Couchbase.Cluster.ConnectAsync(options).ConfigureAwait(false);
            Bucket = await Cluster.BucketAsync("default").ConfigureAwait(false);
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
    }
}
