using System;
using System.Threading.Tasks;

namespace Couchbase.Net.DevGuide
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            // NB: Uncomment the appropriate line to choose which example to run:

            // await new Cas().ExecuteAsync().ConfigureAwait(false);
            // await new AsyncExample().ExecuteAsync().ConfigureAwait(false);
            // await new AsyncBatch().ExecuteAsync().ConfigureAwait(false);
            // await new ConnectionBase().ExecuteAsync().ConfigureAwait(false);
            // await new ConnectionConfig().ExecuteAsync().ConfigureAwait(false);
            // await new Retrieve().ExecuteAsync().ConfigureAwait(false);
            // await new Update().ExecuteAsync().ConfigureAwait(false);
            // await new BulkInsert().ExecuteAsync().ConfigureAwait(false);
            // await new QueryConsistency().ExecuteAsync().ConfigureAwait(false);
            // await new QueryCriteria().ExecuteAsync().ConfigureAwait(false);
            // await new QueryPlaceHolders().ExecuteAsync().ConfigureAwait(false);
            // await new QueryPrepared().ExecuteAsync().ConfigureAwait(false);
            await new Analytics().ExecuteAsync().ConfigureAwait(false);

            // Console.ReadLine();
        }
    }
}
