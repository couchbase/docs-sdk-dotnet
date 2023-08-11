using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Couchbase.Core.Exceptions.KeyValue;

namespace Couchbase.Examples
{
    public class KvOperations
    {
        public async Task ExecuteAsync()
        {
            Console.WriteLine("Running KvOperations samples");

            var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = await bucket.DefaultCollectionAsync();

            {
                // tag::insert[]
                var document = new {foo = "bar", bar = "foo"};
                try {
                    var result = await collection.InsertAsync("document-key", document);
                }
                catch (DocumentExistsException) {
                    Console.WriteLine("Document already exists");
                }
                // end::insert[]
            }

            {
                // tag::insertwithoptions[]
                try {
                    var document = new {foo = "bar", bar = "foo"};

                    var result = await collection.InsertAsync("document-key", document,
                        options =>
                        {
                            options.Expiry(TimeSpan.FromDays(1));
                            options.Timeout(TimeSpan.FromSeconds(5));
                        }
                    );
                }
                catch (DocumentExistsException) {
                    // handle exception
                }
                // end::insertwithoptions[]
            }

            {
                // tag::replacewithcas[]
                var previousResult = await collection.GetAsync("document-key");
                var cas = previousResult.Cas;

                var document = new {foo = "bar", bar = "foo"};

                var result = await collection.ReplaceAsync("document-key", document,
                    options =>
                    {
                        options.Cas(cas);
                        options.Expiry(TimeSpan.FromMinutes(1));
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::replacewithcas[]
            }

            {
                // tag::upsertwithtimeout[]
                var document = new { foo = "bar", bar = "foo" };
                var result = await collection.UpsertAsync("document-key", document,
                    options =>
                    {
                        options.Expiry(TimeSpan.FromMinutes(1));
                        options.Durability(PersistTo.One, ReplicateTo.One);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::upsertwithtimeout[]
            }

            {
                try {
                    // tag::upsertwithdurability[]
                    var document = new { foo = "bar", bar = "foo" };
                    var result = await collection.UpsertAsync("document-key", document,
                        options =>
                        {
                            options.Expiry(TimeSpan.FromMinutes(1));
                            options.Durability(DurabilityLevel.Majority);
                            options.Timeout(TimeSpan.FromSeconds(5));
                        }
                    );
                    // end::upsertwithdurability[]
                }
                catch (Exception)
                {
                    // handle exception. This may be caused in testing by lack of replicas to provide Durability guarantees required.
                }
            }

            {
                // tag::get[]
                var previousResult = await collection.UpsertAsync("string-key", "string value");

                var result = await collection.GetAsync("string-key");
                var content = result.ContentAs<String>();
                // end::get[]
            }

            {
                // tag::getwithtimeout[]
                var result = await collection.GetAsync("string-key",
                    options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                var content = result.ContentAs<string>();
                // end::getwithtimeout[]
            }

            {
                // tag::touch[]
                await collection.TouchAsync("document-key", TimeSpan.FromSeconds(10));
                // end::touch[]
            }

            {
                // tag::touchwithtimeout[]
                await collection.TouchAsync("document-key", TimeSpan.FromSeconds(30),
                    options =>
                    {
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::touchwithtimeout[]
            }

            {
                // tag::binaryincrement[]
                // increment binary value by 1, if document doesn’t exist, seed it at 1
                await collection.Binary.IncrementAsync("binary-key");
                // end::binaryincrement[]
            }

            {
                // tag::binaryincrementwithoptions[]
                await collection.Binary.IncrementAsync("binary-key",
                options =>
                    {
                        options.Delta(1);
                        options.Initial(1000);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::binaryincrementwithoptions[]
            }

            {
                // tag::binarydecrement[]
                // decrement binary value by 1, if document doesn’t exist, seed it at 1
                await collection.Binary.DecrementAsync("binary-key");
                // end::binarydecrement[]
            }

            {
                // tag::binarydecrementwithoptions[]
                await collection.Binary.DecrementAsync("binary-key",
                    options =>
                    {
                        options.Delta(1);
                        options.Initial(1000);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::binarydecrementwithoptions[]
            }

            {
                // tag::remove[]
                var previousResult = await collection.GetAsync("document-key");

                await collection.RemoveAsync("document-key",
                    options =>
                    {
                        options.Cas(previousResult.Cas);
                        options.Timeout(TimeSpan.FromSeconds(5));
                    }
                );
                // end::remove[]
            }

                {
                    Console.WriteLine("\nExample: [named-collection-upsert]");

                    // tag::named-collection-upsert[]
                    var agentScope = await bucket.ScopeAsync("tenant_agent_00");
                    var usersCollection = await agentScope.CollectionAsync("users");

                    var content = new { name = "John Doe", preferred_email = "johndoe111@test123.test" };

                    var result = await usersCollection.UpsertAsync("user-key", content);
                    // end::named-collection-upsert[]

                    Console.WriteLine("cas value: " + result.Cas);
                }
        }
    }
}
