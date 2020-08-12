using System;
using System.Threading.Tasks;
using Couchbase;

namespace sdk_docs_dotnet_examples
{
    class ManagingConnections
    {
        static async Task Main(string[] args)
        {
            {
                // tag::simpleconnect[]
                var cluster = await Cluster.ConnectAsync("couchbase://localhost", "username", "passord");
                var bucket =  await cluster.BucketAsync("travel-sample");
                var collection = bucket.DefaultCollection();

                // You can access multiple buckets using the same Cluster object.
                var anotherBucket = await cluster.BucketAsync("beer-sample");

                // You can access collections other than the default
                // if your version of Couchbase Server supports this feature.
                var customerA = bucket.Scope("customer-a");
                var widgets = customerA.Collection("widgets");

                // For a graceful shutdown, disconnect from the cluster when the program ends.
                await cluster.DisposeAsync();
                // end::simpleconnect[]
            }

            {
                // tag::multinodeconnect[]
                var cluster = await Cluster.ConnectAsync("192.168.56.101,192.168.56.102", "username", "password");
                // end::multinodeconnect[]
            }

            {
                // tag::waitUntilReady[]
                var cluster = await Cluster.ConnectAsync("couchbase://127.0.0.1", "username", "password");
                await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
                var bucket = await cluster.BucketAsync("default");
                var collection = bucket.DefaultCollection();
                // end::waitUntilReady[]
            }
            
            {
                //#tag::enableTls
                var cluster = await Cluster.ConnectAsync(new ClusterOptions
                    {
                        EnableTls = true
                    }
                    .WithConnectionString("couchbase://127.0.0.1")
                    .WithCredentials("username", "password"));
                //#end::enableTls
            }

            {
                //#tag::dnssrv
                var cluster = await Cluster.ConnectAsync(new ClusterOptions
                    {
                        EnableTls = true
                    }
                    .WithConnectionString("couchbases://[YOUR DNS CONNECTION STRING]")
                    .WithCredentials("username", "password"));
                //#end::dnssrv
            }
        }
    }
}
