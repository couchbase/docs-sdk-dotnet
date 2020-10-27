using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions;
using Couchbase.Query;

namespace Couchbase.Net.DevGuide
{
    public class QueryCriteria : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var airport = new
            {
                type = "airport",
                airportname = "Reno International Airport",
                city = "Reno",
                country = "United States"
            };

            await Bucket.DefaultCollection().UpsertAsync("1", airport).ConfigureAwait(false);


            var statement = "SELECT airportname, city, country FROM `default` WHERE type=\"airport\" AND city=\"Reno\"";

            Console.WriteLine("Results from a simple statement:");
            Console.WriteLine(statement);
            var result = await Cluster.QueryAsync<dynamic>(statement,
                new QueryOptions().ScanConsistency(QueryScanConsistency.RequestPlus)).ConfigureAwait(false);
            await foreach (var row in result) Console.WriteLine(row);

            //when there is a server-side error, the server will feed errors in the result.error() collection
            //you can find that out by checking finalSuccess() == false

            try
            {
                var errorResult = (await Cluster.QueryAsync<dynamic>("SELECTE * FROM `travel-sample` LIMIT 3")
                    .ConfigureAwait(false)).MetaData;
                Console.WriteLine(
                    $"With bad syntax, finalSuccess = {errorResult.Status}, errors: {errorResult.Warnings}");
            }
            catch (ParsingFailureException pf)
            {
                Console.WriteLine(pf);
            }
        }

        private new static async Task Main(string[] args)
        {
            await new QueryCriteria().ExecuteAsync().ConfigureAwait(false);
        }
    }
}