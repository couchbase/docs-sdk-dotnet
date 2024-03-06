using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Update : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var collection = Bucket.DefaultCollection();
            var key = "dotnetDevguideExampleUpdate-" + DateTime.Now.Ticks;
            var data = new Data
            {
                Number = 42,
                Text = "Life, the Universe, and Everything",
                Date = DateTime.UtcNow
            };

            // Prepare the document
            // Note that upsert works whether the document exists or not
            await collection.UpsertAsync(key, data).ConfigureAwait(false);

            // Change the data
            data.Number++;
            data.Text = "What's 7 * 6 + 1?";
            data.Date = DateTime.UtcNow;

            try
            {
                // Try to insert under the same key should fail
                await collection.InsertAsync(key, data).ConfigureAwait(false);
            }
            catch (DocumentExistsException)
            {
                Console.WriteLine("Inserting under an existing key fails as expected.");
            }


            // Replace existing document
            // Note this only works if the key already exists
            var replaceResult = await collection.ReplaceAsync(key, data).ConfigureAwait(false);


            // Check that the data was updated
            using var newDocument = await collection.GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("Got: " + newDocument.ContentAs<Data>());
        }

        private new static async Task Main(string[] args)
        {
            await new Update().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
