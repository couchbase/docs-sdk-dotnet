#r "nuget: CouchbaseNetClient, 3.2.0"

using System;
using System.Threading.Tasks;
using Couchbase;

await new ManagingConnections().RunExample();

public class ManagingConnections
{
    public async Task RunExample()
    {
        {
            Console.WriteLine("simpleconnect");
            // tag::simpleconnect[]
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", "username", "password");
            var bucket =  await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();

            // You can access multiple buckets using the same Cluster object.
            var anotherBucket = await cluster.BucketAsync("travel-sample");

            // You can access collections other than the default
            // if your version of Couchbase Server supports this feature.
            var customerA = bucket.Scope("customer-a");
            var widgets = customerA.Collection("widgets");

            // For a graceful shutdown, disconnect from the cluster when the program ends.
            await cluster.DisposeAsync();
            // end::simpleconnect[]
        }

        {
            Console.WriteLine("multinodeconnect");
            // tag::multinodeconnect[]
            var cluster = await Cluster.ConnectAsync("192.168.56.101,192.168.56.102", "username", "password");
            // end::multinodeconnect[]
        }

        {
            Console.WriteLine("waitUntilReady");
            // tag::waitUntilReady[]
            var cluster = await Cluster.ConnectAsync("couchbase://127.0.0.1", "username", "password");
            await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();
            // end::waitUntilReady[]
        }
        
        {
            Console.WriteLine("network-and-timeout");
            // tag::network-and-timeout[]
            var cluster = await Cluster.ConnectAsync(
                "127.0.0.1",
                new ClusterOptions
                {
                    UserName = "username",
                    Password = "password",
                    NetworkResolution = NetworkResolution.External,
                    KvTimeout = TimeSpan.FromSeconds(1)
                }
            );
            // end::network-and-timeout[]
        }
        
        {
            Console.WriteLine("enableTls");
            // tag::enableTls[]
            var cluster = await Cluster.ConnectAsync(new ClusterOptions
                {
                    EnableTls = true
                }
                .WithConnectionString("couchbase://127.0.0.1")
                .WithCredentials("username", "password"));
            // end::enableTls[]
        }

        {
            Console.WriteLine("dnssrv");
            // tag::dnssrv[]
            var cluster = await Cluster.ConnectAsync(new ClusterOptions
                {
                    EnableTls = true
                }
                .WithConnectionString("couchbases://[YOUR DNS CONNECTION STRING]")
                .WithCredentials("username", "password"));
            // end::dnssrv[]
        }
    }
}
