using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Couchbase.Net.DevGuide
{
    internal class AntiPatterns : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);
            var collection = Bucket.DefaultCollection();

            {
                //bad
                var keys = Enumerable.Range(1, 100).Select(i => $"key{i}");

                foreach (var key in keys)
                {
                    var result = await collection.GetAsync(key).ConfigureAwait(false);
                }
            }

            {
                //good
                var tasks = Enumerable.Range(1, 100).Select(i => collection.GetAsync($"key{i}"));
                var results = await Task.WhenAll(tasks).ConfigureAwait(false);
            }
        }

        public async Task Avoid_Synchronously_Awaiting_Foreach_Loops()
        {
            await ConnectAsync().ConfigureAwait(false);
            var collection = Bucket.DefaultCollection();

            var keys = Enumerable.Range(1, 100).Select(i => $"key{i}");

            foreach (var key in keys)
            {
                var result = await collection.GetAsync(key).ConfigureAwait(false);
            }

            var tasks = Enumerable.Range(1, 100).Select(i => collection.GetAsync($"key{i}")).ToAsyncEnumerable();
            //var results = Task.WhenAll(tasks).ConfigureAwait(false);

            await foreach(var task in tasks)
            {
                await task;
            }

            var tokenSource = new CancellationTokenSource();

            {
                //good
#if NET6_0_OR_GREATER
                await Parallel.ForEachAsync(keys, async (key, cancellationToken) =>
                {

                    var result = await collection.GetAsync(key).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
#endif
        }

        public async Task BatchAndPartition()
        {
            await ConnectAsync().ConfigureAwait(false);
            var collection = Bucket.DefaultCollection();
            var keys = Enumerable.Range(1, 100).Select(i => $"key{i}");

#if NET6_0_OR_GREATER
            var partions = keys.Chunk(10);

            foreach (var partition in partions)
            {
                await Parallel.ForEachAsync(keys, async (key, cancellationToken) => {

                    var result = await collection.GetAsync(key).ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
#endif
        }
    }
}
