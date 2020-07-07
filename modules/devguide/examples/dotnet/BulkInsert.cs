using System;
using System.Linq;
using System.Threading.Tasks;

namespace Couchbase.Net.DevGuide
{
    public class BulkInsert : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            // Create 100 Data objects
            var data = Enumerable.Range(1, 100).Select(i => new Data { Number = i });

            var tasks = data.Select(x => Bucket.DefaultCollection().UpsertAsync($"BulkInsert-{x.Number}", x));
            var results = await Task.WhenAll(tasks).ConfigureAwait(false);

            Console.WriteLine($"Wrote {results.Length} docs to Couchbase.");
        }

        static async Task Main(string[] args)
        {
            await new BulkInsert().ExecuteAsync().ConfigureAwait(false); 
        }
    }
}
