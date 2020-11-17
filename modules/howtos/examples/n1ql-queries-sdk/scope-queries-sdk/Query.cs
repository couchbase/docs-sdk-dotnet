using System;
using System.Threading.Tasks;
using Couchbase;

namespace ScopeLevelQuery
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
            var bucket = await cluster.BucketAsync("travel-sample");

            // tag::scope[]
            var myscope = bucket.Scope("us");

            var queryResult = await myscope.QueryAsync<dynamic>("select * from airline");
            await foreach (var row in queryResult)
            {
                Console.WriteLine(row);
            }

            Console.Read();
            // end::scope[]

        }
    }
}
