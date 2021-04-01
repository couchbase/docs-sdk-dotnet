using System;
using System.Threading.Tasks;
using Couchbase.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Counter : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var key = "dotnetDevguideExampleCounter-" + DateTime.Now.Ticks;
            var binaryCollection = Bucket.DefaultCollection().Binary;

            // Try to increment a counter that doesn't exist.
            // This will create the counter with an initial value of 1 regardless of delta specified
            var counter = await binaryCollection.IncrementAsync(key, options => options.Initial(10)).ConfigureAwait(false);
            Console.WriteLine("Initial value = N/A, Increment = 10, Counter value: " + counter.Content);

            // Remove the counter so we can try again
            await Bucket.DefaultCollection().RemoveAsync(key).ConfigureAwait(false);
            Console.WriteLine("Trying again.");

            // Create a counter with an initial value of 13. Again, delta is ignored in this case.
            var counter2 = await binaryCollection.IncrementAsync(key, options => options.Initial(13).Delta(10)).ConfigureAwait(false);
            Console.WriteLine("Initial value = 13, Increment = 10, Counter value: " + counter2.Content);

            // Increment the counter by 10. If the counter exists, the inital value is ignored.
            var counter3 = await binaryCollection.IncrementAsync(key, options => options.Initial(13).Delta(10)).ConfigureAwait(false);
            Console.WriteLine("Initial value = 13, Increment = 10, Counter value: " + counter3.Content);

            // Decrement the counter by 20.
            var counter4 = await binaryCollection.DecrementAsync(key, options => options.Initial(13).Delta(20)).ConfigureAwait(false);
            Console.WriteLine("Initial value = 13, Decrement = 20, Counter value: " + counter4.Content);
        }

        private new static void Main(string[] args)
        {
            new Counter().ExecuteAsync().Wait();
        }
    }
}
