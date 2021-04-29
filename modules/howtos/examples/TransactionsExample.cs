using System;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.KeyValue;
using Couchbase.Query;
using Couchbase.Transactions.Config;
using Couchbase.Transactions.Deferred;
using Couchbase.Transactions.Error;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Couchbase.Transactions.Examples
{
    class Program : IDisposable
    {
        private readonly Transactions _transactions;
        private readonly ICluster _cluster;
        private readonly IBucket _bucket;
        private readonly ICouchbaseCollection _collection;
        private readonly ILogger<Program> _logger;

        public Program(ICluster cluster, IBucket bucket, ICouchbaseCollection collection, Transactions transactions)
        {
            _cluster = cluster;
            _bucket = bucket;
            _collection = collection;
            _transactions = transactions;
        }

        static async Task Main(string[] args)
        {
            // #tag::init[]
            // Initialize the Couchbase cluster
            var options = new ClusterOptions().WithCredentials("Administrator", "password");
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", options).ConfigureAwait(false);
            var bucket = await cluster.BucketAsync("default").ConfigureAwait(false);
            var collection = bucket.DefaultCollection();

            // Create the single Transactions object
            var transactions = Transactions.Create(cluster, TransactionConfigBuilder.Create());
            // #end::init[]

            using var program = new Program(cluster, bucket, collection, transactions);


            Console.WriteLine("Hello World!");
        }

        void Config()
        {
            // #tag::config[]
            var transactions = Transactions.Create(_cluster,
                TransactionConfigBuilder.Create()
                    .DurabilityLevel(DurabilityLevel.PersistToMajority)
                    .Build());
            // #end::config[]
        }

        void ConfigExpired()
        {
            // #tag::config-expiration[]
            Transactions transactions = Transactions.Create(_cluster, TransactionConfigBuilder.Create()
                .ExpirationTime(TimeSpan.FromSeconds(120))
                .Build());
            // #end::config-expiration[]
        }

        void ConfigCleanup(byte[] encoded)
        {
            // #tag::config-cleanup[]
            Transactions transactions = Transactions.Create(_cluster, TransactionConfigBuilder.Create()
                .CleanupClientAttempts(false)
                .CleanupLostAttempts(false)
                .CleanupWindow(TimeSpan.FromSeconds(120))
                .Build());
            // #end::config-cleanup[]
        }

        async Task CreateAsync()
        {
            // #tag::create[]
            try
            {
                await _transactions.RunAsync(async (ctx)=>
                {
                    // 'ctx' is an AttemptContext, which permits getting, inserting,
                    // removing and replacing documents, along with committing and
                    // rolling back the transaction.

                    // ... Your transaction logic here ...

                    // This call is optional - if you leave it off, the transaction
                    // will be committed anyway.
                    await ctx.CommitAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (TransactionCommitAmbiguousException e)
            {
                // The application will of course want to use its own logging rather
                // than Console.WriteLine
                Console.Error.WriteLine("Transaction possibly committed");
                Console.Error.WriteLine(e);
            }
            catch (TransactionFailedException e)
            {
                Console.Error.WriteLine("Transaction did not reach commit point");
                Console.Error.WriteLine(e);
            }
            // #end::create[]
        }

        async Task Examples()
        {
            // #tag::examples[]
            try
            {
                var result = await _transactions.RunAsync(async (ctx) =>
                {
                    // Inserting a doc:
                    var insertedDoc = await ctx.InsertAsync(_collection, "doc-a", new {}).ConfigureAwait(false);

                    // Getting documents:
                    // Use ctx.GetAsync if the document should exist, and the transaction
                    // will fail if it does not
                    var docA = await ctx.GetAsync(_collection, "doc-a").ConfigureAwait(false);

                    // Replacing a doc:
                    var docB = await ctx.GetAsync(_collection, "doc-b").ConfigureAwait(false);
                    var content = docB.ContentAs<dynamic>();
                    content.put("transactions", "are awesome");
                    var replacedDoc = await ctx.ReplaceAsync(docB, content);

                    // Removing a doc:
                    var docC = await ctx.GetAsync(_collection, "doc-c").ConfigureAwait(false);
                    await ctx.RemoveAsync(docC).ConfigureAwait(false);

                    await ctx.CommitAsync().ConfigureAwait(false);
                }).ConfigureAwait(false);
            }
            catch (TransactionCommitAmbiguousException e)
            {
               Console.WriteLine("Transaction possibly committed");
               Console.WriteLine(e);
            }
            catch (TransactionFailedException e)
            {
                Console.WriteLine("Transaction did not reach commit point");
                Console.WriteLine(e);
            }
            // #end::examples[]
        }

        private async Task InsertAsync()
        {
            // #tag::insert[]
            await _transactions.RunAsync(async ctx =>
            {
                var insertedDoc = await ctx.InsertAsync(_collection, "docId", new { }).ConfigureAwait(false);
            }).ConfigureAwait(false);

            // #end::insert[]
        }

        private async Task GetAsync()
        {
            // #tag::get[]
            await _transactions.RunAsync(async ctx =>
            {
                var docId = "a-doc";
                var docOpt = await ctx.GetAsync(_collection, docId).ConfigureAwait(false);
            }).ConfigureAwait(false);
            // #end::get[]
        }

        private async Task GetReadOwnWritesAsync()
        {
            // #tag::getReadOwnWrites[]
            await _transactions.RunAsync(async ctx =>
            {
                var docId = "docId";
                _ = await ctx.InsertAsync(_collection, docId, new { }).ConfigureAwait(false);
                var doc = await ctx.GetAsync(_collection, docId).ConfigureAwait(false);
                Console.WriteLine((object) doc.ContentAs<dynamic>());
            }).ConfigureAwait(false);
            // #end::getReadOwnWrites[]
        }

        async Task ReplaceAsync()
        {
            // #tag::replace[]
            await _transactions.RunAsync(async ctx =>
            {
                var anotherDoc = await ctx.GetAsync(_collection, "anotherDoc").ConfigureAwait(false);
                var content = anotherDoc.ContentAs<dynamic>();
                content.put("transactions", "are awesome");
                _ = await ctx.ReplaceAsync(anotherDoc, content);
            }).ConfigureAwait(false);
            // #end::replace[]
        }

        private async Task RemoveAsync()
        {
            // #tag::remove[]
            await _transactions.RunAsync(async ctx =>
            {
                var anotherDoc = await ctx.GetAsync(_collection, "anotherDoc").ConfigureAwait(false);
                await ctx.RemoveAsync(anotherDoc).ConfigureAwait(false);
            }).ConfigureAwait(false);
            // #end::remove[]
        }

        private async Task CommitAsync()
        {
            // #tag::commit[]
            var result = await _transactions.RunAsync(async (ctx) =>
            {
                var doc = await ctx.GetAsync(_collection, "anotherDoc").ConfigureAwait(false);
                var content = doc.ContentAs<JObject>();
                content.Add("transactions", "are awesome");

                await ctx.ReplaceAsync(doc, content).ConfigureAwait(false);
            }).ConfigureAwait(false);
            // #end::commit[]
        }


        public async Task PlayerHitsMonster(string actionUuid, int damage, string playerId, string monsterId)
        {
            // #tag::full[]
            try
            {
                await _transactions.RunAsync(async (ctx) =>
                {
                    _logger.LogInformation(
                        "Starting transaction, player {playerId} is hitting monster {monsterId} for {damage} points of damage.",
                        playerId, monsterId, damage);

                    var monster = await ctx.GetAsync(_collection, monsterId).ConfigureAwait(false);
                    var player = await ctx.GetAsync(_collection, playerId).ConfigureAwait(false);

                    var monsterContent = monster.ContentAs<JObject>();
                    var playerContent = player.ContentAs<JObject>();

                    var monsterHitPoints = monsterContent.GetValue("hitpoints").ToObject<int>();
                    var monsterNewHitPoints = monsterHitPoints - damage;

                    _logger.LogInformation(
                        "Monster {monsterId} had {monsterHitPoints} hitpoints, took {damage} damage, now has {monsterNewHitPoints} hitpoints.",
                        monsterId, monsterHitPoints, damage, monsterNewHitPoints);

                    if (monsterNewHitPoints <= 0)
                    {
                        // Monster is killed.  The remove is just for demoing, and a more realistic example would set a
                        // "dead" flag or similar.

                        await ctx.RemoveAsync(monster).ConfigureAwait(false);

                        // The player earns experience for killing the monster
                        var experienceForKillingMonster =
                            monsterContent.GetValue("experienceWhenKilled").ToObject<int>();
                        var playerExperience = playerContent.GetValue("experiance").ToObject<int>();
                        var playerNewExperience = playerExperience + experienceForKillingMonster;
                        var playerNewLevel = CalculateLevelForExperience(playerNewExperience);

                        _logger.LogInformation(
                            "Monster {monsterId} was killed.  Player {playerId} gains {experienceForKillingMonster} experience, now has level {playerNewLevel}.",
                            monsterId, playerId, experienceForKillingMonster, playerNewLevel);

                        playerContent["experience"] = playerNewExperience;
                        playerContent["level"] = playerNewLevel;

                        await ctx.ReplaceAsync(player, playerContent).ConfigureAwait(false);
                    }
                    else
                    {
                        _logger.LogInformation("Monster {monsterId} is damaged but alive.", monsterId);

                        // Monster is damaged but still alive
                        monsterContent.Add("hitpoints", monsterNewHitPoints);

                        await ctx.ReplaceAsync(monster, monsterContent).ConfigureAwait(false);
                    }

                    _logger.LogInformation("About to commit transaction");

                }).ConfigureAwait(false);
            }
            catch (TransactionCommitAmbiguousException e)
            {
                _logger.LogWarning("Transaction possibly committed:{0}{1}", Environment.NewLine, e);
            }
            catch (TransactionFailedException e)
            {
                // The operation timed out (the default timeout is 15 seconds) despite multiple attempts to commit the
                // transaction logic.   Both the monster and the player will be untouched.

                // This situation should be very rare.  It may be reasonable in this situation to ignore this particular
                // failure, as the downside is limited to the player experiencing a temporary glitch in a fast-moving MMO.

                // So, we will just log the error
                _logger.LogWarning("Transaction did not reach commit:{0}{1}", Environment.NewLine, e);
            }
            // #end::full[]

            _logger.LogInformation("Transaction is complete");
        }

        private int CalculateLevelForExperience(int exp)
        {
            return exp / 100;
        }

        private async Task Rollback()
        {
            const int costOfItem = 10;
            // #tag::rollback[]
            await _transactions.RunAsync(async (ctx) => {
                var customer = await ctx.GetAsync(_collection, "customer-name").ConfigureAwait(false);

                if (customer.ContentAs<dynamic>().balance < costOfItem)
                {
                    await ctx.RollbackAsync().ConfigureAwait(false);
                }
                // else continue transaction
            }).ConfigureAwait(false);
            // #end::rollback[]
        }

        public class BalanceInsufficientException : Exception { }

        private async Task RollbackCause()
        {
            const int costOfItem = 10;

            // #tag::rollback-cause[]

            try
            {
                await _transactions.RunAsync(async ctx =>
                {
                    var customer = await ctx.GetAsync(_collection, "customer-name").ConfigureAwait(false);

                    if (customer.ContentAs<dynamic>().balance < costOfItem) throw new BalanceInsufficientException();
                    // else continue transaction
                }).ConfigureAwait(false);
            }
            catch (TransactionCommitAmbiguousException e)
            {
                // This exception can only be thrown at the commit point, after the
                // BalanceInsufficient logic has been passed, so there is no need to
                // check getCause here.
                Console.Error.WriteLine("Transaction possibly committed");
                Console.Error.WriteLine(e);
            }
            catch (TransactionFailedException e)
            {
                Console.Error.WriteLine("Transaction did not reach commit point");
            }

            // #end::rollback-cause[]
        }

        async Task CompleteErrorHandling()
        {
            // #tag::full-error-handling[]
            try
            {
                var result = await _transactions.RunAsync(async (ctx) => {
                    // ... transactional code here ...
                });

                // The transaction definitely reached the commit point. Unstaging
                // the individual documents may or may not have completed

                if (result.UnstagingComplete)
                {
                    // Operations with non-transactional actors will want
                    // unstagingComplete() to be true.
                    await _cluster.QueryAsync<dynamic>(" ... N1QL ... ",
                        new QueryOptions()).ConfigureAwait(false);

                    var documentKey = "a document key involved in the transaction";
                    var getResult = await _collection.GetAsync(documentKey).ConfigureAwait(false);
                }
                else
                {
                    // This step is completely application-dependent.  It may
                    // need to throw its own exception, if it is crucial that
                    // result.unstagingComplete() is true at this point.
                    // (Recall that the asynchronous cleanup process will
                    // complete the unstaging later on).
                }
            }
            catch (TransactionCommitAmbiguousException err)
            {
                // The transaction may or may not have reached commit point
                Console.Error.WriteLine("Transaction returned TransactionCommitAmbiguous and" +
                        " may have succeeded, logs:");

                // Of course, the application will want to use its own logging rather
                // than Console.Error
                Console.Error.WriteLine(err);
            }
            catch (TransactionFailedException err)
            {
                // The transaction definitely did not reach commit point
                Console.Error.WriteLine("Transaction failed with TransactionFailed, logs:");
                Console.Error.WriteLine(err);
            }
            // #end::full-error-handling[]
        }

        async Task LogOnFailure()
        {
            // #tag::logging[]
            try
            {
                var result = await transactions.RunAsync(async ctx => {
                    // ... transactional code here ...
                });
            }
            catch (TransactionFailedException err)
            {
                // ... log the error as you normally would
                // then include the logs
                foreach (var logLine in err.Result.Logs)
                {
                    Console.Error.WriteLine(logLine);
                }
            }
            // #end::logging[]
        }

        async Task CompleteLogging()
        {
            // #tag::full-logging[]
            //Logging dependencies
            var services = new ServiceCollection();
            services.AddLogging(builder =>
            {
                builder.AddFile(AppContext.BaseDirectory);
                builder.AddConsole();
            });
            await using var provider = services.BuildServiceProvider();
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<Program>();

            //create the transactions object and add the ILoggerFactory
            var transactions = Transactions.Create(_cluster,
                TransactionConfigBuilder.Create().LoggerFactory(loggerFactory));
            try
            {
                var result = await transactions.RunAsync(async ctx => {
                    // ... transactional code here ...
                });
            }
            catch (TransactionCommitAmbiguousException err)
            {
                // The transaction may or may not have reached commit point
                logger.LogInformation("Transaction returned TransactionCommitAmbiguous and" +
                            " may have succeeded, logs:");
                Console.Error.WriteLine(err);
            }
            catch (TransactionFailedException err)
            {
                // The transaction definitely did not reach commit point
                logger.LogInformation("Transaction failed with TransactionFailed, logs:");
                Console.Error.WriteLine(err);
            }
            // #end::full-logging[]
        }

        public void Dispose()
        {
            _cluster.Dispose();
            _transactions.Dispose();
        }
    }
}
