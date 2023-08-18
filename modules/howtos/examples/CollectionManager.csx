// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script CollectionManager.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Management.Users;
using Couchbase.Management.Collections;
// Stops ambiguous reference errors
using ScopeNotFoundException = Couchbase.Core.Exceptions.ScopeNotFoundException;

await new CollectionManager().ExecuteAsync();
public class CollectionManager
{
    public async Task ExecuteAsync()
    {
        Console.WriteLine("scopeAdmin");
        {
            // tag::scopeAdmin[]
            ICluster clusterAdmin = await Cluster.ConnectAsync(
                "couchbase://your-ip", "Administrator", "password");
            IUserManager users =  clusterAdmin.Users;

            var user = new User("scopeAdmin") {
                Password = "password",
                DisplayName = "Manage Scopes [travel-sample:*]",
                Roles = new List<Role>() {
                    new Role("scope_admin", "travel-sample"),
                    new Role("data_reader", "travel-sample")}
            };

            await users.UpsertUserAsync(user);
            // end::scopeAdmin[]
        }
        // wait for the cluster to add the new user
        Thread.Sleep(1000);
        ICluster cluster = await Cluster.ConnectAsync("couchbase://your-ip", "scopeAdmin", "password");
        IBucket bucket = await cluster.BucketAsync("travel-sample");

        // tag::create-collection-manager[]
        ICouchbaseCollectionManager collectionMgr = bucket.Collections;
        // end::create-collection-manager[]
        {
            Console.WriteLine("create-scope");
            // tag::create-scope[]
            try {
                await collectionMgr.CreateScopeAsync("example-scope");
            }
            catch (ScopeExistsException) {
                Console.WriteLine("The scope already exists");
            }
            // end::create-scope[]
        }
        {
            Console.WriteLine("create-collection");
            // tag::create-collection[]
            var spec = new CollectionSpec("example-scope", "example-collection");

            try {
                await collectionMgr.CreateCollectionAsync(spec);
            }
            catch (CollectionExistsException) {
                Console.WriteLine("Collection already exists");
            }
            catch (ScopeNotFoundException) {
                Console.WriteLine("The specified parent scope doesn't exist");
            }
            // end::create-collection[]
            
            Console.WriteLine("listing-scope-collection");
            // tag::listing-scope-collection[]
            var scopes = await collectionMgr.GetAllScopesAsync();
            foreach (ScopeSpec scopeSpec in scopes) {
                Console.WriteLine($"Scope: {scopeSpec.Name}");
                
                foreach (CollectionSpec collectionSpec in scopeSpec.Collections) {
                    Console.WriteLine($" - {collectionSpec.Name}");
                }
            }
            // end::listing-scope-collection[]

            Console.WriteLine("drop-collection");
            // tag::drop-collection[]
            try {
                await collectionMgr.DropCollectionAsync(spec);
            }
            catch (CollectionNotFoundException) {
                Console.WriteLine("The specified collection doesn't exist");
            }
            catch (ScopeNotFoundException) {
                Console.WriteLine("The specified parent scope doesn't exist");
            }
            // end::drop-collection[]
        }
        {
            Console.WriteLine("drop-scope");
            // tag::drop-scope[]
            try {
                await collectionMgr.DropScopeAsync("example-scope");
            }
            catch (ScopeNotFoundException) {
                Console.WriteLine("The specified scope doesn't exist");
            }
            // end::drop-scope[]
        }
    }
}
