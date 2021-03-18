using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Cas : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var key = "dotnetDevguideExampleCas-" + DateTime.Now.Ticks;
            var data = new Data
            {
                Number = 0
            };

            // Set the initial number value to 0
            await Bucket.DefaultCollection().UpsertAsync(key, data).ConfigureAwait(false);

            // Try to increment the number 1000 times without using CAS (10 threads x 100 increments)
            // We would expect the result to be Number == 1000 at the end of the process.
            var tasksWithoutCas = Enumerable.Range(1, 10).Select(i => UpdateNumberWithoutCas(key, 100));
            await Task.WhenAll(tasksWithoutCas).ConfigureAwait(false);

            // Check if the actual result is 1000 as expected
            var result = await Bucket.DefaultCollection().GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("Expected number = 1000, actual number = " + result.ContentAs<Data>().Number);

            // Set the initial number value back to 0
            await Bucket.DefaultCollection().UpsertAsync(key, data).ConfigureAwait(false);

            // Now try to increment the number 1000 times with CAS
            var tasksWithCas = Enumerable.Range(1, 10).Select(i => UpdateNumberWithCas(key, 100));
            await Task.WhenAll(tasksWithCas).ConfigureAwait(false);

            // Check if the actual result is 1000 as expected
            var result2 = await Bucket.DefaultCollection().GetAsync(key).ConfigureAwait(false);
            Console.WriteLine("Expected number = 1000, actual number = " + result2.ContentAs<Data>().Number);
        }

        private async Task UpdateNumberWithoutCas(string key, int count)
        {
            for (var i = 0; i < count; i++)
            {
                // Get the document
                var result = await Bucket.DefaultCollection().GetAsync(key).ConfigureAwait(false);

                // Update the document
                var doc = result.ContentAs<Data>();
                doc.Number++;

                // Store the document back without CAS
                await Bucket.DefaultCollection().ReplaceAsync(key, doc).ConfigureAwait(false);
            }
        }

        private async Task UpdateNumberWithCas(string key, int count)
        {
            for (var i = 0; i < count; i++)
            {
                var retries = 100;
                do
                {
                    // Get the document
                    var result = await Bucket.DefaultCollection().GetAsync(key).ConfigureAwait(false);
                    var doc = result.ContentAs<Data>();

                    // Update the document
                    doc.Number++;

                    try
                    {
                        // Store the document back without CAS
                        await Bucket.DefaultCollection().ReplaceAsync(key, doc, options => options.Cas(result.Cas))
                            .ConfigureAwait(false);
                    }
                    catch (DocumentExistsException)
                    {
                    }
                } while (retries-- > 0);
            }
        }

        private new static async Task Main(string[] args)
        {
            await new Cas().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
