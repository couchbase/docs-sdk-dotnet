using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Users;

namespace sdk_docs_dotnet_examples
{
    public class ErrorHandlingcs
    {
        private static int MaxRetries = 5;
        static async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "username", "password");
            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();

            // tag::retry[]
            var attempts = MaxRetries; // eg 5
            while (attempts-- > 0)
            {
                // will throw KeyNotfoundException if key doesn't exist
                var document = await collection.GetAsync("doc_id");
                var user = document.ContentAs<User>();
                user.Email = "john.smith@couchbase.com";

                try
                {
                    await collection.ReplaceAsync("doc_id", user);

                    // replace succeeded, break from loop
                    break;
                }
                catch (CouchbaseException exception)
                {
                    switch (exception)
                    {
                        // unrecoverable error (network failure, etc)
                        case NetworkErrorException _:
                            throw;

                        //case other unrecoverable exceptions
                    }
                }

                // wait 100 milliseconds before trying again
                Task.Delay(100);
            }
            // end::retry[]
		}

        public class User
        {
            public string Email { get; set; }
        }
    }
}
