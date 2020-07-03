using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Couchbase.Net.DevGuide
{
    public class ConnectionConfig
    {
        protected ICluster Cluster;
        protected IBucket Bucket;

        private async Task ConnectAsync()
        {
            var options = new ConfigurationBuilder()
                .AddJsonFile("config.json")
                .Build()
                .GetSection("couchbase")
                .Get<ClusterOptions>();

            Cluster = await Couchbase.Cluster.ConnectAsync(options).ConfigureAwait(false);
            Bucket = await Cluster.BucketAsync("default").ConfigureAwait(false);
        }

        private async Task DisconnectAsync()
        {
            await Bucket.DisposeAsync().ConfigureAwait(false);
            await Cluster.DisposeAsync().ConfigureAwait(false);
        }

        public virtual async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            Console.WriteLine("Connected to bucket '{0}'", Bucket.Name);
        }

        static async Task Main(string[] args)
        {
            await new ConnectionConfig().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
