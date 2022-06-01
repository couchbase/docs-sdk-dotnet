// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script ErrorHandling.csx
//

#r "nuget: CouchbaseNetClient, 3.3.1"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Analytics;
using Couchbase.Query;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.Core.Retry;
using Couchbase.KeyValue;

await new ErrorHandling().ExampleAsync();

public class ErrorHandling
{
    public async Task ExampleAsync()
    {
         var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
         var bucket = await cluster.BucketAsync("default");
         var collection = bucket.DefaultCollection(); 

        {
            Console.WriteLine("[readonly]");
            // tag::readonly[]
            var queryResult = await cluster.QueryAsync<dynamic>("SELECT * FROM `travel-sample`", new QueryOptions().Readonly(true));

            var analyticsResult = await cluster.AnalyticsQueryAsync<dynamic>("SELECT * FROM `travel-sample`.inventory.airport",
            new AnalyticsOptions().Readonly(true));
             // end::readonly[]
        }

        {
            Console.WriteLine("[getfetch]");
            // tag::getfetch[]
            // This will raise a `CouchbaseException` and propagate it
            var result = await collection.GetAsync("my-document-id");

            // Rethrow with a custom exception type
            try {
                await collection.GetAsync("my-document-id");
            } catch (CouchbaseException ex) {
                throw new Exception("Couchbase lookup failed", ex);
            }
            // end::getfetch[]
        }

        {
            Console.WriteLine("[getcatch]");
            // tag::getcatch[]
            try {
                await collection.GetAsync("my-document-id");
            } catch (DocumentNotFoundException) {
                await collection.InsertAsync("my-document-id", new {my ="value"});
            } catch (CouchbaseException ex) {
                throw new Exception("Couchbase lookup failed", ex);
            }
            // end::getcatch[]
        }

        {
            Console.WriteLine("[tryupsert]");
            // tag::tryupsert[]
            for (int i = 0; i < 10; i++) {
                try {
                    await collection.UpsertAsync("docid", new {my ="value"});
                break;
                } catch (TimeoutException) {
                    // propagate, since time budget's up
                break;
                } catch (CouchbaseException ex) {
                    Console.WriteLine($"Failed: {ex}, retrying.");
                    // don't break, so retry
                }
            }
            // end::tryupsert[]
        }

        {
            Console.WriteLine("[customglobal]");
            IRetryStrategy myCustomStrategy = null;
            // tag::customglobal[]
            var clusterOptions = new ClusterOptions().WithRetryStrategy(myCustomStrategy);
            // end::customglobal[]
        }

        {
            Console.WriteLine("[customgrequest]");
            IRetryStrategy myCustomStrategy = null;
            // tag::customreq[]
            await collection.GetAsync("doc-id", new GetOptions().RetryStrategy(myCustomStrategy));
            // end::customreq[]
        }
    }

    // tag::customclass[]
    public class MyCustomRetryStrategy : IRetryStrategy {
        public RetryAction RetryAfter(IRequest request, RetryReason reason) {
            return RetryAction.Duration(null);
        }
    }
    // end::customclass[]

    // tag::failfastcircuit[]
    public class MyCustomRetryStrategy2 : BestEffortRetryStrategy {
        public new RetryAction RetryAfter(IRequest request, RetryReason reason){
            if (reason == RetryReason.CircuitBreakerOpen) {
                //passing null will ensure RetryAction.Retry is false
                return RetryAction.Duration(null); 
            }
            return base.RetryAfter(request, reason);
        }
    }
    // end::failfastcircuit[]
}
