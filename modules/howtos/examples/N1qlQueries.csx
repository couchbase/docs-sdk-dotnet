// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script N1qlQueries.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Query;

var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
var bucket = await cluster.BucketAsync("travel-sample");
var collection = bucket.DefaultCollection();

await new N1qlQueries().PositionalParameterExample(cluster);
await new N1qlQueries().NamedParameterExample(cluster);
await new N1qlQueries().ScanConsistencyNotBoundedExample(cluster, collection);
await new N1qlQueries().ScanConsistencyRequestPlusExample(cluster, collection);
await new N1qlQueries().ScanConsistencyAtPlusExample(cluster, collection);
await new N1qlQueries().QueryResultExample(cluster);
await new N1qlQueries().ExecutionTimeExample(cluster);
await new N1qlQueries().ClientContextIdExample(cluster);
await new N1qlQueries().ReadonlyExample(cluster);

await cluster.DisposeAsync();
public class N1qlQueries
{
    public async Task ReadonlyExample(ICluster cluster)
    {
        // tag::readonlyexample[]
        var result = await cluster.QueryAsync<dynamic>("SELECT 1=1", 
            options => options.Readonly(true));
        // end::readonlyexample[]            
    }

    public async Task ClientContextIdExample(ICluster cluster)
    {
        // tag::clientcontextexample[]
        var result = await cluster.QueryAsync<dynamic>("SELECT 1=1", 
            options => options.ClientContextId($"azure-resource-group-1-{Guid.NewGuid()}"));
        // end::clientcontextexample[]
    }

    public async Task ExecutionTimeExample(ICluster cluster)
    {
        // tag::executiontime[]
        var result = await cluster.QueryAsync<dynamic>("SELECT 1=1", options => options.Metrics(true));

        var metrics = result.MetaData.Metrics;

        Console.WriteLine($"Execution time: {metrics.ExecutionTime}");
        // end::executiontime[]
    }

    public async Task QueryResultExample(ICluster cluster)
    {
        // tag::queryresultexample[]
        var resultDynamic = await cluster.QueryAsync<dynamic>("SELECT t.* FROM `travel-sample` t WHERE t.type='landmark' LIMIT 10");
        IAsyncEnumerable<dynamic> dynamicRows = resultDynamic.Rows;

        var resultPoco = await cluster.QueryAsync<Landmark>("SELECT t.* FROM `travel-sample` t WHERE t.type='landmark' LIMIT 10");
        IAsyncEnumerable<Landmark> pocoRows = resultPoco.Rows;
        // end::queryresultexample[]

        /*
        // examples of query results MetaData
        string requestId = resultPoco.MetaData.RequestId;
        string clientContextId = resultPoco.MetaData.ClientContextId;
        QueryStatus queryStatus = resultPoco.MetaData.Status;
        QueryMetrics queryMetrics = resultPoco.MetaData.Metrics;
        dynamic signature = resultPoco.MetaData.Signature;
        List<QueryWarning> warnings = resultPoco.MetaData.Warnings;
        dynamic profile = resultPoco.MetaData.Profile;
        */
    }

    public class Landmark
    {
        public string Name {get; set;}
        public string Url {get;set;}
        public string Content {get;set;}
        public string City {get;set;}
    }

    public async Task ScanConsistencyAtPlusExample(ICluster cluster, ICouchbaseCollection collection)
    {
        // tag::atplus[]
        // create / update document (mutation)
        var upsertResult = await collection.UpsertAsync("doc1", new { name = "Mike AtPlus", type = "user" });

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
            var name = row.name;        // "Mike AtPlus"
            Console.WriteLine($"{name}");
        }
    }

    public async Task ScanConsistencyRequestPlusExample(ICluster cluster, ICouchbaseCollection collection)
    {
        // create / update document (mutation)
        var upsertResult = await collection.UpsertAsync("doc2", new { name = "Mike RequestPlus", type = "user" });

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
            var name = row.name;        // "Mike RequestPlus"
            Console.WriteLine($"{name}");
        }
    }

    public async Task ScanConsistencyNotBoundedExample(ICluster cluster, ICouchbaseCollection collection)
    {
        // create / update document (mutation)
        var upsertResult = await collection.UpsertAsync("doc3", new { name = "Mike NotBounded", type = "user" });

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
            var name = row.name;        // "Mike NotBounded"
            Console.WriteLine($"{name}");
        }
    }

    public async Task NamedParameterExample(ICluster cluster)
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

    public async Task PositionalParameterExample(ICluster cluster)
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