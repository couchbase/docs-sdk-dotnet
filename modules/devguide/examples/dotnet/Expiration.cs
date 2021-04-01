using System;
using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Expiration : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var key = "dotnetDevguideExampleExpiration-" + DateTime.Now.Ticks;
            var collection = Bucket.DefaultCollection();

            // Creating a document with an time-to-live (expiration) of 2 seconds
            await collection.UpsertAsync(key, "Hello world!", options => options.Timeout(TimeSpan.FromSeconds(2))).ConfigureAwait(false);

            // Retrieving immediately
            var result = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("[{0:HH:mm:ss.fff}] Got: '{1}'!", DateTime.Now, result.ContentAs<string>());

            // Waiting 4 seconds
            await Task.Delay(4000).ConfigureAwait(false);

            // Retrieving after a 4 second delay
            var result2 = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("[{0:HH:mm:ss.fff}] Got: '{1}'!", DateTime.Now, result2.ContentAs<string>());

            // Creating an item with 1 second TTL
            await collection.UpsertAsync(key, "Hello world!", options=>options.Timeout(TimeSpan.FromSeconds(1))).ConfigureAwait(false);

            // Retrieving the item and extending the TTL to 2 seconds with getAndTouch
            var result3 = await collection.GetAndTouchAsync(key, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Console.WriteLine("[{0:HH:mm:ss.fff}] Got: '{1}'!", DateTime.Now, result3.ContentAs<string>());

            // Waiting 4 seconds again
            await Task.Delay(4000).ConfigureAwait(false);

            var result4 = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("[{0:HH:mm:ss.fff}] Got: '{1}'!", DateTime.Now, result4.ContentAs<string>());


            // Creating an item without expiration
            await collection.UpsertAsync(key, "Hello world!").ConfigureAwait(false);

            // Updating the TTL with Touch
            await collection.TouchAsync(key, TimeSpan.FromSeconds(2)).ConfigureAwait(false);
            Console.WriteLine("Touched key for {0}", TimeSpan.FromSeconds(2));

            // Waiting 4 seconds yet again
            await Task.Delay(4000).ConfigureAwait(false);

            var result6 = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("[{0:HH:mm:ss.fff}] Got: '{1}'!", DateTime.Now, result6.ContentAs<string>());
        }

        private new static async Task Main(string[] args)
        {
           await new Expiration().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
