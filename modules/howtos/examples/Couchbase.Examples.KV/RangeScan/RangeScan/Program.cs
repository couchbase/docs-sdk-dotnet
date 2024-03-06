using Couchbase;
using Couchbase.KeyValue;
using Couchbase.KeyValue.RangeScan;
using Serilog;
using Serilog.Extensions.Logging;

Serilog.Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Information()
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

await RangeScanAllDocuments(inventoryScope);
await RangeScanPrefixScan(inventoryScope);

async Task RangeScanAllDocuments(IScope scope)
{
    var collection = scope.Collection("hotel");
    // tag::rangeScanAllDocuments[]
    IAsyncEnumerable<IScanResult> results = collection.ScanAsync(new RangeScan());
    
    await foreach (var scanResult in results)
    {
        Log.Information(scanResult.Id);
        Log.Information(scanResult.ContentAs<Hotel>().ToString());
    }

    // alternate declaration
    var scan2 = new RangeScan(from: ScanTerm.Inclusive("id001"), to: ScanTerm.Inclusive("id999"));
    // end::rangeScanAllDocuments[]
}

async Task RangeScanPrefixScan(IScope scope)
{
    var collection = scope.Collection("hotel");
    // tag::rangeScanPrefix[]
    IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
        new PrefixScan("alice::")
    );
    
    await foreach (var scanResult in results)
    {
        Log.Information(scanResult.Id);
    }
    // end::rangeScanPrefix[]
}

async Task RangeScanSamplingScan(IScope scope)
{
    var collection = scope.Collection("hotel");
    // tag::rangeScanSample[]
    IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
        new SamplingScan(limit: 100)
    );
    
    await foreach (var scanResult in results)
    {
        Log.Information(scanResult.Id);
    }
    // end::rangeScanSample[]
}
async Task RangeScanAllDocumentIds(IScope scope)
{
    var collection = scope.Collection("hotel");
    // tag::rangeScanAllDocumentIds[]
    IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
        new RangeScan(),
        new ScanOptions().IdsOnly(true));
    
    await foreach (var scanResult in results)
    {
        Log.Information(scanResult.Id);
    }
    // end::rangeScanAllDocumentIds[]
}

record Hotel(string name, string title, string address);
