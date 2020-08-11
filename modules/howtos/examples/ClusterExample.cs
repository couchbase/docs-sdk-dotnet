using System;
using System.Threading.Tasks;
using Couchbase;

namespace sdk_docs_dotnet_examples
{
    //#tag::waitUntilReady
    public class ClusterExample
    {
        public async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://127.0.0.1", "username", "password");
            await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
            var bucket = await cluster.BucketAsync("default");
            var collection = bucket.DefaultCollection();
            //..
        }
    }
    //#end::waitUntilReady

    //#tag::waitUntilReadyBucket
    public class ClusterExample2
    {
        public async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://127.0.0.1", "username", "password");
            var bucket = await cluster.BucketAsync("default");
            await bucket.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
            var collection = bucket.DefaultCollection();
            //..
        }
    }
    //#end::waitUntilReadyBucket
}
