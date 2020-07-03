using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions.KeyValue;
using Couchbase.KeyValue;

namespace Couchbase.Net.DevGuide
{
    public class Durability : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var key = "dotnetDevguideExampleDurability-" + DateTime.Now.Ticks;
            var data = new Data
            {
                Number = 42,
                Text = "Life, the Universe, and Everything",
                Date = DateTime.UtcNow
            };
            
            // The ReplicateTo parameter must be less than or equal to the number of replicas 
            // you have configured. Assuming that 3 replicas are configured, the following call
            // waits for replication to 3 replicas and persistence to 4 nodes in total.

            try
            {
                var result = await Bucket.DefaultCollection()
                    .UpsertAsync(key, data, options => options.Durability(PersistTo.Four, ReplicateTo.Three))
                    .ConfigureAwait(false);

                Console.WriteLine("Durability met!");
            }
            catch (DurabilityLevelNotAvailableException)
            {
                //Durability could match cluster configuration
                Console.WriteLine("Write failed - not enough replicas configured to satisfy durability requirements");
                throw;
            }
            catch (DurabilityAmbiguousException)
            {
                // It's possible for a write to succeed, but not satisfy durability.
                // For example, writing with PersistTo.Two and ReplicateTo.Zero on a 1-node cluster.
                Console.WriteLine("Write failed - not enough replicas configured to satisfy durability requirements");
            }
            catch (CouchbaseException e)
            {
                Console.WriteLine($"An error has occured: {e}");
            }
            
            // Wait for the write to be persisted to disk on one (normally the master) node.
            var result2 = await Bucket.DefaultCollection().UpsertAsync(key, data, options => options.Durability(PersistTo.Four, ReplicateTo.Three)).ConfigureAwait(false);
            Console.WriteLine("Doc persisted to disk!");
        }

        static async Task Main(string[] args)
        {
           await new Durability().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
