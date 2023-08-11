#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
// #tag::using[]
using System.Threading.Tasks;
using Couchbase;
// #end::using[]

await new StartUsing().ExampleUsing();
class StartUsing
{
    public async Task ExampleUsing()
    {

        // #tag::connect[]
        var cluster = await Cluster.ConnectAsync(
            // Update these credentials for your Local Couchbase instance!
            "couchbase://your-ip",
            "Administrator",
            "password");
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
