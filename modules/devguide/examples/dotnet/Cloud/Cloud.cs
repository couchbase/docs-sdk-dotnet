using System;
using System.Threading.Tasks;

namespace Couchbase.Net.DevGuide.Cloud;
// #end::using[]

public class Progam
{
    public static async Task Main(string[] args)
    {
        await new CloudExample().Main();
    }
}

class CloudExample
{
    public async Task Main()
    {
        // #tag::connect[]
        var options = new ClusterOptions
        {
            // Update these credentials for your Capella instance
            UserName = "username",
            Password = "Password!123",
        };

        // Sets a pre-configured profile called "wan-development" to help avoid latency issues
        // when accessing Capella from a different Wide Area Network
        // or Availability Zone (e.g. your laptop).
        options.ApplyProfile("wan-development");

        var cluster = await Cluster.ConnectAsync(
            // Update these credentials for your Capella instance
            "couchbases://cb.<your-endpoint>.cloud.couchbase.com",
            options
        );
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
        using var getResult = await collection.GetAsync("my-document-key");

        Console.WriteLine(getResult.ContentAs<dynamic>());
        // #end::upsert-get[]

        // tag::n1ql-query[]
        // Call the QueryAsync() function on the scope object and store the result.
        var inventoryScope = bucket.Scope("inventory");
        var queryResult = await inventoryScope.QueryAsync<dynamic>("SELECT * FROM airline WHERE id = 10");
        
        // Iterate over the rows to access result data and print to the terminal.
        await foreach (var row in queryResult) {
            Console.WriteLine(row);
        }
        // end::n1ql-query[]
    }
}