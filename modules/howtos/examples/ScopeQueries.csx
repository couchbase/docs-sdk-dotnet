// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script ScopeQueries.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Threading.Tasks;
using Couchbase;

await new ScopeQueries().ExcecuteAsync();

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