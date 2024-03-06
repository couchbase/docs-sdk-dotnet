using Couchbase;
using Couchbase.Core.Exceptions;
using Couchbase.KeyValue;
using Couchbase.Search;
using Couchbase.Search.Queries.Compound;
using Couchbase.Search.Queries.Range;
using Couchbase.Search.Queries.Simple;
using Couchbase.Search.Queries.Vector;
using Serilog;
using Serilog.Extensions.Logging;

Serilog.Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Verbose()
    .WriteTo.Console()
    .CreateLogger();

var clusterOptions = new ClusterOptions()
{
    ConnectionString = "couchbase://localhost",
    UserName = "Administrator",
    Password = "password",
    EnableDnsSrvResolution = false,
};

clusterOptions = clusterOptions.WithLogging(new SerilogLoggerFactory());

var exampleCluster = await Cluster.ConnectAsync(clusterOptions);
await exampleCluster.WaitUntilReadyAsync(TimeSpan.FromSeconds(10));
var travelSample = await exampleCluster.BucketAsync("travel-sample");
var inventoryScope = travelSample.Scope("inventory");
await Task.Delay(1000);

await ClusterGetAllIndexes(exampleCluster);
await ClusterSearch(exampleCluster);
await ScopedGetAllIndexes(inventoryScope);
await ScopedSearch(inventoryScope);

async Task ScopedGetAllIndexes(IScope scope)
{
    // tag::scopedGetAllIndexes[]
    Log.Information("Fetching all search indexes on given scope...");
    var searchIndexes = scope.SearchIndexes;
    var allScopedIndexes = await searchIndexes.GetAllIndexesAsync();
    foreach (var idx in allScopedIndexes)
    {
        Serilog.Log.Information("Search Index: {idx} in scope {scope}", idx.Name, scope.Name);
    }
    // end::scopedGetAllIndexes[]
}

async Task ScopedSearch(IScope scope)
{
    // Assumes the scoped index "index-hotel-description" has already been created.
    // see https://docs.couchbase.com/server/7.6/fts/fts-creating-index-from-UI-classic-editor-dynamic.html
    // for information on how to create a scoped index.
    // tag::scopedFtsSearch[]
    var searchResult = await scope.SearchAsync("index-hotel-description",
        SearchRequest.Create(
            new MatchQuery("swanky")),
        new SearchOptions().Limit(10));
    // end::scopedFtsSearch[]
    // tag::scopedEnumerateHits[]
    foreach (var hit in searchResult.Hits)
    {
        string documentId = hit.Id;
        double score = hit.Score;
        Log.Information("Hit: {id}: {score}", documentId, score);
    }
    // end::scopedEnumerateHits[]
    // tag::scopedEnumerateFacets[]
    foreach (var keyValuePair in searchResult.Facets)
    {
        var facet = keyValuePair.Value;
        var name = facet.Name;
        var total = facet.Total;
        Log.Information("Facet: {key}={name},{total}", keyValuePair.Key, name, total);
    }
    // end::scopedEnumerateFacets[]
}

async Task ScopedDateSearch(IScope scope)
{
    // tag::scopedDateSearch[]
    var searchResult = await scope.SearchAsync("index-name",
        SearchRequest.Create(
            new DateRangeQuery()
                .Start(DateTime.Parse("2021-01-01"), inclusive: true)
                .End(DateTime.Parse("2021-02-01"), inclusive: false)
            ), new SearchOptions().Limit(10));
    // end::scopedDateSearch[]
    foreach (var row in searchResult)
    {
        Log.Information("result: {row}", row.Locations?.ToString());
    }
}

async Task ScopedConjunctionSearch(IScope scope)
{
    // tag::scopedConjunctionSearch[]
    var searchResult = await scope.SearchAsync("index-name",
        SearchRequest.Create(
            new ConjunctionQuery(
            new DateRangeQuery()
                .Start(DateTime.Parse("2021-01-01"), inclusive: true)
                .End(DateTime.Parse("2021-02-01"), inclusive: false),
            new MatchQuery("swanky"))
        ), new SearchOptions().Limit(10));
    // end::scopedConjunctionSearch[]
    foreach (var row in searchResult)
    {
        Log.Information("result: {row}", row.Locations?.ToString());
    }
}

async Task ScopedConsistentWithSearch(IScope scope)
{
    // tag::scopedConsistentWithSearch[]
    var searchResult = await scope.SearchAsync("index-hotel-description",
        SearchRequest.Create(
            new MatchQuery("swanky")
        ), new SearchOptions()
            .Limit(10)
            .ScanConsistency(SearchScanConsistency.RequestPlus)
        );
    // end::scopedConsistentWithSearch[]
    foreach (var row in searchResult)
    {
        Log.Information("result: {row}", row.Locations?.ToString());
    }
}

async Task ClusterSearch(ICluster cluster)
{
    try
    {
        // tag::clusterFtsSearch[]
        var searchResult = await cluster.SearchAsync(
            "travel-sample.inventory.index-hotel-description",
            SearchRequest.Create(new MatchQuery("swanky")),
            new SearchOptions().Limit(10)
        );
        // end::clusterFtsSearch[]
        foreach (var row in searchResult)
        {
            Log.Information("result: {row}", row.Locations?.ToString());
        }
    }
    catch (IndexNotFoundException e)
    {
        Log.Error(e, $"IndexNotFoundException: {nameof(ClusterSearch)}");
    }
}

async Task ClusterGetAllIndexes(ICluster cluster)
{
    Log.Information("Fetching all search indexes on cluster...");
    // tag::clusterGetAllIndexes[]
    var searchIndexes = cluster.SearchIndexes;
    var allScopedIndexes = await searchIndexes.GetAllIndexesAsync();
    foreach (var idx in allScopedIndexes)
    {
        Serilog.Log.Information("Search Index: {idx}", idx.Name);
    }
    // end::clusterGetAllIndexes[]
}

async Task ScopedVectorQuery(IScope scope)
{
    // in actual usage, these vectors would be generated by some other AI toolkit.
    float[] preGeneratedVectors = new[] { 0.001f, 0.002f, 0.003f };
    //tag::scopedVector1[]
    var searchRequest = SearchRequest.Create(
        VectorSearch.Create(new VectorQuery("vector_field", preGeneratedVectors))
    );
    
    var searchResult = scope.SearchAsync("travel-vector-index", searchRequest, new SearchOptions());
    //end::scopedVector1[]
}

async Task ScopedVectorQueryMultiple(IScope scope)
{
    // in actual usage, these vectors would be generated by some other AI toolkit.
    float[] preGeneratedVectors = new[] { 0.001f, 0.002f, 0.003f };
    var vectorQuery = new VectorQuery("vector-field-1", preGeneratedVectors);
    var anotherVectorQuery = new VectorQuery("vector-field-2", preGeneratedVectors);
    //tag::scopedVector2[]
    var searchRequest = SearchRequest.Create(
        VectorSearch.Create(new[]
            {
            vectorQuery.WithOptions(
                new VectorQueryOptions().WithNumCandidates(2).WithBoost(0.3f)),
            anotherVectorQuery.WithOptions(
                new VectorQueryOptions().WithNumCandidates(5).WithBoost(0.7f)),
            })
    );
    
    // or with C# record syntax
    var searchRequest2 = SearchRequest.Create(
        new VectorSearch(new[]
            {
                vectorQuery 
                    with { Options = new VectorQueryOptions() { NumCandidates = 2, Boost = 0.3f } },
                anotherVectorQuery
                    with { Options = new VectorQueryOptions() { NumCandidates = 5, Boost = 0.3f } },
            },
            Options: new VectorSearchOptions(VectorQueryCombination.And)
        ));
    
    var searchResult = scope.SearchAsync("travel-vector-index", searchRequest, new SearchOptions());
    //end::scopedVector2[]
}

async Task ScopedVectorWithFts(IScope scope)
{
    // in actual usage, these vectors would be generated by some other AI toolkit.
    float[] preGeneratedVectors = new[] { 0.001f, 0.002f, 0.003f };
    //tag::scopedVectorWithFts[]
    var searchRequest = new SearchRequest(
        SearchQuery: new MatchQuery("swanky"),
        VectorSearch: VectorSearch.Create(new VectorQuery("vector_field", preGeneratedVectors))
    );
    
    var searchResult = scope.SearchAsync("travel-index", searchRequest, new SearchOptions());
    //end::scopedVectorWithFts[]
}
