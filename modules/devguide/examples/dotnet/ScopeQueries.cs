using System;
using System.Threading.Tasks;
using Couchbase;

namespace Couchbase.Docs.Examples.Howtos.ScopeQueries;


class ScopeQueries
{
    public async Task ExcecuteAsync()
    {
        var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
        var bucket = await cluster.BucketAsync("travel-sample");

        // tag::scope[]
        var myscope = bucket.Scope("inventory");

        var queryResult = await myscope.QueryAsync<dynamic>("select * from airline LIMIT 10", new Couchbase.Query.QueryOptions());
        await foreach (var row in queryResult)
        {
            Console.WriteLine(row);
        }

        // end::scope[]
        await cluster.DisposeAsync();

    }
}