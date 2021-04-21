using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Couchbase;
using Couchbase.Management.Users;
using Couchbase.Management.Collections;

namespace Couchbase.Examples
{
    public class CollectionManager
    {

        async Task<ICouchbaseCollectionManager> getCollectionManager (String username, String password) {
            Console.WriteLine("create-collection-manager");

            // tag::create-collection-manager[]
            ICluster cluster = await Cluster.ConnectAsync("couchbase://localhost", username, password);
            IBucket bucket = await cluster.BucketAsync("travel-sample");
            ICouchbaseCollectionManager collectionMgr = bucket.Collections;
            // end::create-collection-manager[]

            return collectionMgr;
        }

        public async Task ExecuteAsync()
        {
            ICluster cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
            IUserManager users =  cluster.Users;

            Console.WriteLine("bucketAdmin");
            // tag::bucketAdmin[]
            {
                var user = new User("bucketAdmin");
                user.Password = "password";
                user.DisplayName = "Bucket Admin [travel-sample]";
                user.Roles = new List<Role>() {
                    new Role("bucket_admin", "travel-sample") };
                await users.UpsertUserAsync(user);
            }
            // end::bucketAdmin[]

            {
                var collectionMgr = await getCollectionManager("bucketAdmin", "password");

                // tag::create-scope[]
                try {
                    await collectionMgr.CreateScopeAsync("example-scope");
                }
                catch (ScopeExistsException) {
                    Console.WriteLine("The scope already exists");
                }
                // end::create-scope[]
            }

            Console.WriteLine("scopeAdmin");
            // tag::scopeAdmin[]
            {
                var user = new User("scopeAdmin");
                user.Password = "password";
                user.DisplayName = "Manage Collections in Scope [travel-sample:*]";
                user.Roles = new List<Role>() {
                    new Role("scope_admin", "travel-sample"),
                    new Role("data_reader", "travel-sample")};
                await users.UpsertUserAsync(user);
            }
            // end::scopeAdmin[]

            {
                Console.WriteLine("create-collection");
                var collectionMgr = await getCollectionManager("scopeAdmin", "password");

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
                var collectionMgr = await getCollectionManager("bucketAdmin", "password");
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


}
