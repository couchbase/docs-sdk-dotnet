// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script QueryIndexManagerExample.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Query;
using Couchbase.Core.Exceptions;

await new QueryIndexManagerExample().ExampleAsync();

public class QueryIndexManagerExample
{
    public async Task ExampleAsync()
    {
        // tag::creating-index-mgr[]
        var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
        var bucket = await cluster.BucketAsync("travel-sample");
        var scope = await bucket.ScopeAsync("tenant_agent_01");
        var collection = await scope.CollectionAsync("users");

        var queryIndexMgr = collection.QueryIndexes;
        // end::creating-index-mgr[]

        {
            Console.WriteLine("[primary]");
            // tag::primary[]
            await queryIndexMgr.CreatePrimaryIndexAsync(
                new CreatePrimaryQueryIndexOptions()
                    // Set this if you wish to use a custom name
                    // .IndexName("custom_name")
                    .IgnoreIfExists(true)
            );
            // end::primary[]
        }

        {
            Console.WriteLine("[secondary]");
            // tag::secondary[]
            try
            {
                await queryIndexMgr.CreateIndexAsync(
                    "tenant_agent_01_users_email",
                    new[] { "preferred_email" },
                    new CreateQueryIndexOptions()
                );
            }
            catch (IndexExistsException)
            {
                Console.WriteLine("Index already exists!");
            }
            // end::secondary[]
        }

        {
            Console.WriteLine("[defer-indexes]");
            // tag::defer-indexes[]
            try
            {
                // Create a deferred index
                await queryIndexMgr.CreateIndexAsync(
                    "tenant_agent_01_users_phone",
                    new[] { "preferred_phone" },
                    new CreateQueryIndexOptions()
                        .Deferred(true)
                );

                // Build any deferred indexes within `travel-sample`.tenant_agent_01.users
                await queryIndexMgr.BuildDeferredIndexesAsync(
                    new BuildDeferredQueryIndexOptions()
                );

                // Wait for indexes to come online
                TimeSpan duration = TimeSpan.FromSeconds(10);
                await queryIndexMgr.WatchIndexesAsync(
                    new[] { "tenant_agent_01_users_phone" },
                    duration,
                    new WatchQueryIndexOptions()
                );
            }
            catch (IndexExistsException)
            {
                Console.WriteLine("Index already exists!");
            }
            // end::defer-indexes[]
        }

        {
            Console.WriteLine("[drop-primary-or-secondary-index]");
            // tag::drop-primary-or-secondary-index[]
            // Drop primary index
            await queryIndexMgr.DropPrimaryIndexAsync(
                new DropPrimaryQueryIndexOptions()
            );

            // Drop secondary index
            await queryIndexMgr.DropIndexAsync(
                "tenant_agent_01_users_email",
                new DropQueryIndexOptions()
            );
            // end::drop-primary-or-secondary-index[]
        }
    }
}
