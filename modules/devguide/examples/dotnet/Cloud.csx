// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script Cloud.csx
//

#r "nuget: CouchbaseNetClient, 3.2.8-pre"
// #r "nuget: System.Text.Json, 6.0.2"
// #r "nuget: Microsoft.Extensions.Logging.Abstractions, 6.0.1"
// #r "nuget: CouchbaseNetClient, 3.3.0"

using System;
// #tag::using[]
using System.Threading.Tasks;
using Couchbase;
// #end::using[]

await new CloudExample().Main();

class CloudExample
{
    public async Task Main()
    {
        // #tag::connect[]
        var cluster = await Cluster.ConnectAsync(
            // Update these credentials for your Capella instance!
            "couchbases://cb.njg8j7mwqnvwjqah.cloud.couchbase.com",
            "username",
            "Password!123");
        // #end::connect[]

        // #tag::bucket[]
        // get a bucket reference
        var bucket = await cluster.BucketAsync("travel-sample");
        // #end::bucket[]

        // #tag::collection[]
        // get a user-defined collection reference
        var scope = await bucket.ScopeAsync("tenant_agent_00");
        var collection = await scope.CollectionAsync("users");
        // #end::collection[]

        // #tag::upsert-get[]
        // Upsert Document
        var upsertResult = await collection.UpsertAsync("my-document-key", new { Name = "Ted", Age = 31 });
        var getResult = await collection.GetAsync("my-document-key");

        Console.WriteLine(getResult.ContentAs<dynamic>());
        // #end::upsert-get[]

        // tag::n1ql-query[]
        // Call the QueryAsync() function on the cluster object and store the result.
        var queryResult = await cluster.QueryAsync<dynamic>("select \"Hello World\" as greeting");
        
        // Iterate over the rows to access result data and print to the terminal.
        await foreach (var row in queryResult) {
            Console.WriteLine(row);
        }
        // end::n1ql-query[]
    }
}
