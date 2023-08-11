// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script QueryIndexManagerExample.csx
//

#r "nuget: CouchbaseNetClient, 3.3.0"

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
        var queryIndexMgr = cluster.QueryIndexes;
        // end::creating-index-mgr[]

        {
            Console.WriteLine("[primary]");
            // tag::primary[]
            await queryIndexMgr.CreatePrimaryIndexAsync(
                "travel-sample",
                new CreatePrimaryQueryIndexOptions()
                    .ScopeName("tenant_agent_01")
                    .CollectionName("users")
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
                    "travel-sample",
                    "tenant_agent_01_users_email",
                    new[] { "preferred_email" },
                    new CreateQueryIndexOptions()
                        .ScopeName("tenant_agent_01")
                        .CollectionName("users")
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
                    "travel-sample",
                    "tenant_agent_01_users_phone",
                    new[] { "preferred_phone" },
                    new CreateQueryIndexOptions()
                        .ScopeName("tenant_agent_01")
                        .CollectionName("users")
                        .Deferred(true)
                );

                // Build any deferred indexes within `travel-sample`.tenant_agent_01.users
                await queryIndexMgr.BuildDeferredIndexesAsync(
                    "travel-sample",
                    new BuildDeferredQueryIndexOptions()
                        .ScopeName("tenant_agent_01")
                        .CollectionName("users")
                );

                // Wait for indexes to come online
                await queryIndexMgr.WatchIndexesAsync(
                    "travel-sample",
                    new[] { "tenant_agent_01_users_phone" },
                    new WatchQueryIndexOptions()
                        .ScopeName("users")
                        .CollectionName("tenant_agent_01")
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
                "travel-sample",
                new DropPrimaryQueryIndexOptions()
                    .ScopeName("tenant_agent_01")
                    .CollectionName("users")
            );

            // Drop secondary index
            await queryIndexMgr.DropIndexAsync(
                "travel-sample",
                "tenant_agent_01_users_email",
                new DropQueryIndexOptions()
                    .ScopeName("tenant_agent_01")
                    .CollectionName("users")
            );
            // end::drop-primary-or-secondary-index[]
        }
    }
}
