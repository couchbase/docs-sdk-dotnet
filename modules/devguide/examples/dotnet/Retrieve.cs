using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Retrieve : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var collection = Bucket.DefaultCollection();
            var key = "dotnetDevguideExampleRetrieve-" + DateTime.Now.Ticks;
            var data = new Data
            {
                Number = 42,
                Text = "Life, the Universe, and Everything",
                Date = DateTime.UtcNow
            };

            try
            {
                // Get non-existent document.
                // Note that it's enough to check the Status property,
                // We're only checking all three to show they exist.
                await collection.GetAsync(key).ConfigureAwait(false);
            }
            catch (DocumentNotFoundException)
            {
                Console.WriteLine("As expected, the document doesn't exist!");
            }

            // Prepare a string value
            await collection.UpsertAsync(key, "Hello Couchbase!").ConfigureAwait(false);

            // Get a string value
            var nonDocResult = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("Found: " + nonDocResult.ContentAs<string>());

            // Prepare a JSON document value
            await collection.UpsertAsync(key, data).ConfigureAwait(false);

            // Get a JSON document string value
            var docResult = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("Found: " + docResult.ContentAs<Data>());
        }

        private new static async Task Main(string[] args)
        {
            await new Retrieve().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
