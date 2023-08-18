// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script Collections.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Management.Collections;

await new CollectionsExample().ExampleAsync();

public class CollectionsExample
{
    public async Task ExampleAsync()
    {
        var cluster = await Cluster.ConnectAsync(
            "couchbase://your-ip",
            "Administrator", "password");

        var bucket = await cluster.BucketAsync("travel-sample");

        // create collection in default scope
        var collectionMgr = bucket.Collections;

        var spec = new CollectionSpec("_default", "bookings");

        try {
            await collectionMgr.CreateCollectionAsync(spec);
        }
        catch (CollectionExistsException) {
            Console.WriteLine("Collection already exists");
        }

        // tag::collections_1[]
        var collection_in_default_scope = await bucket.CollectionAsync("bookings");
        // end::collections_1[]

        // tag::collections_2[]
        var tenant_scope = await bucket.ScopeAsync("tenant_agent_00");
        var collection_in_scope = await tenant_scope.CollectionAsync("bookings");
        // end::collections_2[]

        Console.WriteLine("Done");
    }
}

