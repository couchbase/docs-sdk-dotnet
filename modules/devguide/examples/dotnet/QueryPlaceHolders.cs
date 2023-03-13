using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Couchbase.Query;

namespace Couchbase.Net.DevGuide
{
    /**
 * Example of Querying using placeholders with SQL++ (N1QL) in Java for the Couchbase Developer Guide.
 */
    public class QueryPlaceHolders : ConnectionBase
    {
        private static string PlaceholderStatement = "SELECT airportname FROM `travel-sample` WHERE city=$1 AND type=\"airport\"";

        private async Task<IQueryResult<dynamic>> QueryCity(String city)
        {
            //the placeholder values can be provided as a JSON array (if using $1 syntax)
            // or map-like JSON object (if using $name syntax)
            var result = await Cluster.QueryAsync<dynamic>(
                PlaceholderStatement,
                new QueryOptions().Parameter(city)).ConfigureAwait(false);

            return result;
        }

        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var airport = new
            {
                type="airport",
                airportname = "Reno International Airport",
                city = "Reno",
                country = "United States"
            };

            await Bucket.DefaultCollection().UpsertAsync("1", airport).ConfigureAwait(false);

            airport = new
            {
                type = "airport",
                airportname = "Los Angeles International Airport",
                city = "Los Angeles",
                country = "United States"
            };

            await Bucket.DefaultCollection().UpsertAsync("2", airport).ConfigureAwait(false);

            airport = new
            {
                type = "airport",
                airportname = "Culver City Airport",
                city = "Los Angeles",
                country = "United States"
            };

            await Bucket.DefaultCollection().UpsertAsync("3", airport).ConfigureAwait(false);

            airport = new
            {
                type = "airport",
                airportname = "Dallas International Airport",
                city = "Dallas",
                country = "United States"
            };

            await Bucket.DefaultCollection().UpsertAsync("4", airport).ConfigureAwait(false);

            Console.WriteLine("Airports in Reno: ");
            await foreach (var row in await QueryCity("Reno").ConfigureAwait(false))
            {
                Console.WriteLine(row);
            }

            Console.WriteLine("Airports in Dallas: ");
            await foreach (var row in await QueryCity("Dallas").ConfigureAwait(false))
            {
                Console.WriteLine(row);
            }

            Console.WriteLine("Airports in Los Angeles: ");
            await foreach (var row in await QueryCity("Los Angeles").ConfigureAwait(false))
            {
                Console.WriteLine(row);
            }
        }

        private new static async Task Main(string[] args)
        {
            await new QueryPlaceHolders().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
