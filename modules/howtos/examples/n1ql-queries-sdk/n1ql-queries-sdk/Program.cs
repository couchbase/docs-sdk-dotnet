using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;

namespace n1ql_queries_sdk
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");

            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();

            // await PositionalParameterExample(cluster);

            // await NamedParameterExample(cluster);

            // await ScanConsistencyNotBoundedExample(cluster, collection);

            // await ScanConsistencyRequestPlusExample(cluster, collection);

            await ScanConsistencyAtPlusExample(cluster, collection);

            await cluster.DisposeAsync();
        }

        private static async Task ScanConsistencyAtPlusExample(ICluster cluster, ICouchbaseCollection collection)
        {
            // tag::atplus[]
            // create / update document (mutation)
            var upsertResult = await collection.UpsertAsync("id", new { name = "Mike AtPlus", type = "user" });

            // create mutation state from mutation results
            var state = MutationState.From(upsertResult);

            // use mutation state with query option
            var result = await cluster.QueryAsync<dynamic>(
                "SELECT t.* FROM `travel-sample` t WHERE t.type=$1",
                options => options.ConsistentWith(state)
                    .Parameter("user")
            );
            // end::atplus[]

            // check query was successful
            if (result.MetaData.Status != QueryStatus.Success)
            {
                // error
            }

            // iterate over rows
            // NOTE: because the query is using AtPlus, the new user will be indexed, but notice
            // the extra step of creating the 'state' object and passing it in as a 'ConsistentWith' option
            await foreach (var row in result)
            {
                // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                var name = row.name;        // "Mike"
                Console.WriteLine($"{name}");
            }
        }

        private static async Task ScanConsistencyRequestPlusExample(ICluster cluster, ICouchbaseCollection collection)
        {
            // create / update document (mutation)
            var upsertResult = await collection.UpsertAsync("id", new { name = "Mike RequestPlus", type = "user" });

            // tag::requestplus[]
            var result = await cluster.QueryAsync<dynamic>(
                "SELECT t.* FROM `travel-sample` t WHERE t.type=$1",
                options => options.Parameter("user")
                    .ScanConsistency(QueryScanConsistency.RequestPlus)
            );
            // end::requestplus[]

            // check query was successful
            if (result.MetaData.Status != QueryStatus.Success)
            {
                // error
            }

            // iterate over rows
            // NOTE: because the query is using RequestPlus, the new user will be indexed, but the query may take slightly longer to finish
            await foreach (var row in result)
            {
                // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                var name = row.name;        // "Mike"
                Console.WriteLine($"{name}");
            }
        }

        private static async Task ScanConsistencyNotBoundedExample(ICluster cluster, ICouchbaseCollection collection)
        {
            // create / update document (mutation)
            var upsertResult = await collection.UpsertAsync("id", new { name = "Mike NotBounded", type = "user" });

            // tag::notbounded[]
            var result = await cluster.QueryAsync<dynamic>(
                "SELECT t.* FROM `travel-sample` t WHERE t.type=$1",
                options => options.Parameter("user")
                    .ScanConsistency(QueryScanConsistency.NotBounded)
            );
            // end::notbounded[]

            // check query was successful
            if (result.MetaData.Status != QueryStatus.Success)
            {
                // error
            }

            // iterate over rows
            // NOTE: because the query is using NotBounded, the new user may not be indexed yet
            await foreach (var row in result)
            {
                // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                var name = row.name;        // "Mike"
                Console.WriteLine($"{name}");
            }
        }

        private static async Task NamedParameterExample(ICluster cluster)
        {
            // tag::namedparameter[]
            var result = await cluster.QueryAsync<dynamic>(
                "SELECT t.* FROM `travel-sample` t WHERE t.type=$type",
                options => options.Parameter("type", "landmark")
            );
            // end::namedparameter[]

            // tag::namedparameter2[]
            // check query was successful
            if (result.MetaData.Status != QueryStatus.Success)
            {
                // error
            }

            // iterate over rows
            await foreach (var row in result)
            {
                // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                var name = row.name;        // "Hollywood Bowl"
                var address = row.address;      // "4 High Street, ME7 1BB"
                Console.WriteLine($"{name},{address}");
            }
            // end::namedparameter2[]
        }

        private static async Task PositionalParameterExample(ICluster cluster)
        {
            // tag::positional[]
            var result = await cluster.QueryAsync<dynamic>(
                "SELECT t.* FROM `travel-sample` t WHERE t.type=$1",
                options => options.Parameter("landmark")
            );
            // end::positional[]

            // check query was successful
            if (result.MetaData.Status != QueryStatus.Success)
            {
                // error
            }

            // iterate over rows
            await foreach (var row in result)
            {
                // each row is an instance of the Query<T> call (e.g. dynamic or custom type)
                var name = row.name;        // "Hollywood Bowl"
                var address = row.address;      // "4 High Street, ME7 1BB"
                Console.WriteLine($"{name},{address}");
            }
        }
    }
}
