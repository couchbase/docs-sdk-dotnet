using Couchbase;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;

internal static class SubDocumentReplicaExamples
{
    static async Task LookupInAnyReplica(ICouchbaseCollection collection)
    {
        // tag::lookup-in-any-replica[]
        try
        {
            var result = await collection.LookupInAnyReplicaAsync(
                "hotel_10138",
                specs => specs.Get("geo.lat")
            );

            var geoLat = result.ContentAs<string>(0);
            Console.Out.WriteLine($"getFunc: Latitude={geoLat}");
        }
        catch (PathNotFoundException)
        {
            Console.Error.WriteLine("The version of the document" +
                               " on the server node that responded quickest" +
                               " did not have the requested field.");
        }
        catch (DocumentUnretrievableException)
        {
            Console.Error.WriteLine("Document was not present" +
                                    " on any server node");
        }
        // end::lookup-in-any-replica[]
    }

    static async Task LookupInAllReplicas(ICouchbaseCollection collection)
    {
        // tag::lookup-in-all-replicas[]
        IAsyncEnumerable<ILookupInResult> result = collection.LookupInAllReplicasAsync(
            "hotel_10138",
            specs => specs.Get("geo.lat"));

        await foreach (var replicaResult in result)
        {
            try
            {
                var geoLat = replicaResult.ContentAs<string>(0);
                Console.Out.WriteLine($"getFunc: Latitude={geoLat}");
            }
            catch (PathNotFoundException)
            {
                Console.Error.WriteLine("The version of the document" +
                                        " on the server node that responded quickest" +
                                        " did not have the requested field.");
            }
        }
        // end::lookup-in-all-replicas[]
    }
}
