using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Couchbase.Net.DevGuide
{
    public class Analytics : ConnectionBase
    {
        private class TestRequest
        {
            [JsonProperty("greeting")]
            public string Greeting { get; set; }
        }

        public override async Task ExecuteAsync()
        {
            await ConnectAsync("travel-sample").ConfigureAwait(false);

            {
                const string statement = "SELECT \"hello\" as greeting;";
                var result = await Cluster.AnalyticsQueryAsync<TestRequest>(statement);
                await foreach (var row in result)
                {
                    Console.WriteLine("Result: " + row.Greeting);
                }
            }

            var cluster = Cluster;

            {
                Console.WriteLine("\nHandle Collection");
                // tag::handle-collection[]
                var result = await cluster.AnalyticsQueryAsync<dynamic>(
                    "SELECT airportname, country FROM `travel-sample`.inventory.airport WHERE country='France' LIMIT 3");

                await foreach (var row in result)
                {
                    Console.WriteLine("Result: " + row);
                }
                // end::handle-collection[]
            }

            /*
            {
                Console.WriteLine("\nHandle Scope");
                // tag::handle-scope[]
                IBucket bucket = await cluster.BucketAsync("travel-sample").ConfigureAwait(false);
                var scope = await bucket.ScopeAsync("inventory");
                var result = await scope.AnalyticsQueryAsync<dynamic>(
                    "SELECT airportname, country FROM airport WHERE country='France' LIMIT 2");
                // end::handle-scope[]

                await foreach (var row in result)
                {
                    Console.WriteLine("Result: " + row);
                }
            }
            */
        }

        private new static async Task Main(string[] args)
        {
            await new Analytics().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
