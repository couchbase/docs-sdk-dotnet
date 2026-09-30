using Couchbase.KeyValue;
using Couchbase.KeyValue.RangeScan;

namespace RangeScan;

internal static class RangeScanExamples
{
    static async Task RangeScanAllDocuments(IScope scope)
    {
        var collection = scope.Collection("hotel");
        // tag::rangeScanAllDocuments[]
        IAsyncEnumerable<IScanResult> results = collection.ScanAsync(new Couchbase.KeyValue.RangeScan.RangeScan());

        await foreach (var scanResult in results)
        {
            Console.WriteLine(scanResult.Id);
            Console.WriteLine(scanResult.ContentAs<Hotel>().ToString());
        }

        // alternate declaration
        var scan2 = new Couchbase.KeyValue.RangeScan.RangeScan(from: ScanTerm.Inclusive("id001"), to: ScanTerm.Inclusive("id999"));
        // end::rangeScanAllDocuments[]
    }

    static async Task RangeScanPrefixScan(IScope scope)
    {
        var collection = scope.Collection("hotel");
        // tag::rangeScanPrefix[]
        IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
            new PrefixScan("alice::")
        );

        await foreach (var scanResult in results)
        {
            Console.WriteLine(scanResult.Id);
        }
        // end::rangeScanPrefix[]
    }

    static async Task RangeScanSamplingScan(IScope scope)
    {
        var collection = scope.Collection("hotel");
        // tag::rangeScanSample[]
        IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
            new SamplingScan(limit: 100)
        );

        await foreach (var scanResult in results)
        {
            Console.WriteLine(scanResult.Id);
        }
        // end::rangeScanSample[]
    }

    static async Task RangeScanAllDocumentIds(IScope scope)
    {
        var collection = scope.Collection("hotel");
        // tag::rangeScanAllDocumentIds[]
        IAsyncEnumerable<IScanResult> results = collection.ScanAsync(
            new Couchbase.KeyValue.RangeScan.RangeScan(),
            new ScanOptions().IdsOnly(true));

        await foreach (var scanResult in results)
        {
            Console.WriteLine(scanResult.Id);
        }
        // end::rangeScanAllDocumentIds[]
    }
}

record Hotel(string name, string title, string address);