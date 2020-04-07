using System;
// #tag::using[]
using System.Threading.Tasks;
using Couchbase;
// #end::using[]

namespace examples
{
    class StartUsing
    {
        static async Task Main(string[] args)
        {

            // #tag::connect[]
            await using var cluster = await Cluster.ConnectAsync("couchbase://localhost", "username", "password");
            // #end::connect[]


            // #tag::bucket[]
            // get a bucket reference
            var bucket = await cluster.BucketAsync("bucket-name");
            // #end::bucket[]

            // #tag::collection[]
            // get a collection reference
            var collection = bucket.DefaultCollection();
            // #end::collection[]

            // #tag::upsert-get[]
            // Upsert Document
            var upsertResult = await collection.UpsertAsync("my-document", new { Name = "Ted", Age = 31 });
            var getResult = await collection.GetAsync("my-document");

            Console.WriteLine(getResult.ContentAs<dynamic>());
            // #end::upsert-get[]

            // TODO: update this to not require the QueryOptions object, NCBC-2459 NCBC-2458
            // TODO: also, note the full example in the start-using adoc
            // #tag::n1ql-query[]
            var queryResult = await cluster.QueryAsync<dynamic>("select \"Hello World\" as greeting", new Couchbase.Query.QueryOptions());
            await foreach (var row in queryResult) {
                Console.WriteLine(row);
            }
            // #end::n1ql-query[]

        }
    }
}
