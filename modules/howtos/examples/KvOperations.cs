using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;

namespace sdk_docs_dotnet_examples
{
    public class KvOperations
    {
        public static async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", "username", "password");
            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();

            {
                // #tag::insert[]
                var document = new {foo = "bar", bar = "foo"};
                var result = await collection.InsertAsync("document-key", document);
                // #end::insert[]
            }

            {
                // #tag::insertwithoptions[]
                var document = new {foo = "bar", bar = "foo"};
                var result = await collection.InsertAsync("document-key", document,
                    options =>
                    {
                        options.Expiry(TimeSpan.FromDays(1));
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::insertwithoptions[]
            }

            {
                // #tag::replacewithcas[]
                var document = new {foo = "bar", bar = "foo"};
                var result = await collection.ReplaceAsync("document-key", document,
                    options =>
                    {
                        options.Cas(12345);
                        options.Expiry(TimeSpan.FromMinutes(1));
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::replacewithcas[]
            }

            {
                // #tag::upsertwithtimeout[]
                var document = new { foo = "bar", bar = "foo" };
                var result = await collection.UpsertAsync("document-key", document,
                    options =>
                    {
                        options.Expiry(TimeSpan.FromMinutes(1));
                        options.Durability(PersistTo.One, ReplicateTo.One);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::upsertwithtimeout[]
            }

            {
                // #tag::upsertwithdurability[]
                var document = new { foo = "bar", bar = "foo" };
                var result = await collection.UpsertAsync("document-key", document,
                    options =>
                    {
                        options.Expiry(TimeSpan.FromMinutes(1));
                        options.Durability(DurabilityLevel.Majority);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::upsertwithdurability[]
            }

            {
                // #tag::get[]
                var result = await collection.GetAsync("document-key");
                var content = result.ContentAs<string>();
                // #end::get[]
            }

            {
                // #tag::getwithtimeout[]
                var result = await collection.GetAsync("document-key",
                    options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                var content = result.ContentAs<string>();
                // #end::getwithtimeout[]
            }

            {
                // #tag::remove[]
                await collection.RemoveAsync("document-key",
                    options =>
                    {
                        options.Cas(12345);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::remove[]
            }

            {
                // #tag::touch[]
                await collection.TouchAsync("document-key", TimeSpan.FromSeconds(10));
                // #end::touch[]
            }

            {
                // #tag::touchwithtimeout[]
                await collection.TouchAsync("document-key", TimeSpan.FromSeconds(30),
                    options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::touchwithtimeout[]
            }

            {
                // #tag::binaryincrement[]
                // increment binary value by 1, if document doesn’t exist, seed it at 1
                await collection.Binary.IncrementAsync("document-key");
                // #end::binaryincrement[]
            }

            {
                // #tag::binaryincrementwithoptions[]
                await collection.Binary.IncrementAsync("document-key",
                options =>
                    {
                        options.Delta(1);
                        options.Initial(1000);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                    );
                // #end::binaryincrementwithoptions[]
            }

            {
                // #tag::binarydecrement[]
                // decrement binary value by 1, if document doesn’t exist, seed it at 1
                await collection.Binary.DecrementAsync("document-key");
                // #end::binarydecrement[]
            }

            {
                // #tag::binarydecrementwithoptions[]
                await collection.Binary.DecrementAsync("document-key",
                    options =>
                    {
                        options.Delta(1);
                        options.Initial(1000);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // #end::binarydecrementwithoptions[]
            }
        }
    }
}
