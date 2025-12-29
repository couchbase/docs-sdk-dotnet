using System;
using System.Threading.Tasks;
using Couchbase;await ExampleUsing();
async Task ExampleUsing()
{
    var cluster = await Cluster.ConnectAsync(
        // Update these credentials for your Local Couchbase instance!
        "couchbase://localhost",
        "Administrator",
        "password");

    // get a bucket reference
    var bucket = await cluster.BucketAsync("travel-sample");

    // get a user-defined collection reference
    var scope = await bucket.ScopeAsync("tenant_agent_00");
    var collection = await scope.CollectionAsync("users");

    // Upsert Document
    var upsertResult = await collection.UpsertAsync("my-document-key", new { Name = "Ted", Age = 31 });
    var getResult = await collection.GetAsync("my-document-key");

    Console.WriteLine(getResult.ContentAs<dynamic>());

    // Call the QueryAsync() function on the scope object and store the result.
    var inventoryScope = bucket.Scope("inventory");
    var queryResult = await inventoryScope.QueryAsync<dynamic>("SELECT * FROM airline WHERE id = 10");

    // Iterate over the rows to access result data and print to the terminal.
    await foreach (var row in queryResult) {
        Console.WriteLine(row);
    }
}
