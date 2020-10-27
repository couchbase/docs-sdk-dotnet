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

            var result = await Cluster.AnalyticsQueryAsync<TestRequest>(statement).ConfigureAwait(false);

            await foreach (var row in result)
            {
                Console.WriteLine(row);
            }
        }

        static async Task Main(string[] args)
        {
            await new Analytics().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
