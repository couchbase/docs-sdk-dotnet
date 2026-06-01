using System;
using System.Threading.Tasks;
using Couchbase;

namespace Couchbase.Docs.Examples.Howtos.ClusterExample;


// tag::waitUntilReady[]
public class ClusterExample
{
    public async Task ExecuteAsync()
    {
        var cluster = await Cluster.ConnectAsync("your-ip", "Administrator", "password");
        await cluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
        var bucket = await cluster.BucketAsync("travel-sample");
        var collection = bucket.DefaultCollection();
        //..
    }
}
// end::waitUntilReady[]

// tag::waitUntilReadyBucket[]
public class ClusterExample2
{
    public async Task ExecuteAsync()
    {
        var cluster = await Cluster.ConnectAsync("your-ip", "Administrator", "password");
        var bucket = await cluster.BucketAsync("travel-sample");
        await bucket.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
        var collection = bucket.DefaultCollection();
        //..
    }
}
// end::waitUntilReadyBucket[]