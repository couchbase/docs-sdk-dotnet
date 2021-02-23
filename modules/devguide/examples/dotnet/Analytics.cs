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
            const string statement = "SELECT \"hello\" as greeting;";

            await ConnectAsync("travel-sample").ConfigureAwait(false);

            var result = await Cluster.AnalyticsQueryAsync<TestRequest>(statement);

            await foreach (var row in result)
            {
                Console.WriteLine("Result: " + row.Greeting);
            }

        }

        static async Task Main(string[] args)
        {
            await new Analytics().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
