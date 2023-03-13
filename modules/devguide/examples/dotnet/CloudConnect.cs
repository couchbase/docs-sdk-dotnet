using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Query;
using Couchbase.Management.Query;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Logging;
// using Serilog;
// using Serilog.Extensions.Logging;

namespace net3
{
    public class Program2
    {
        static async Task Main(string[] args)
        {
            // Update this to your cluster
            var endpoint = "cb.13d1a4bc-31a8-49c6-9ade-74073df0799f.dp.cloud.couchbase.com";
            var bucketName = "couchbasecloudbucket";
            var username = "user";
            var password = "password";
            // User Input ends here.

            // IServiceCollection serviceCollection = new ServiceCollection();
            // serviceCollection.AddLogging(builder => builder.AddFilter(level => level >= LogLevel.Trace));

            // var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            // loggerFactory.AddFile("Logs/myapp-{Date}.txt", LogLevel.Debug);

            // Initialize the Connection
            var opts = new ClusterOptions().WithCredentials(username, password);
            // opts = opts.WithLogging(loggerFactory);
            opts.IgnoreRemoteCertificateNameMismatch = true;

            var cluster = await Cluster.ConnectAsync("couchbases://"+endpoint, opts);
            var bucket = await cluster.BucketAsync(bucketName);
            var collection = bucket.DefaultCollection();

            // Store a Document
            var upsertResult = await collection.UpsertAsync("king_arthur", new {
                Name = "Arthur",
                Email = "kingarthur@couchbase.com",
                Interests = new[] { "Holy Grail", "African Swallows" }
            });

            // Load the Document and print it
            var getResult = await collection.GetAsync("king_arthur");
            Console.WriteLine(getResult.ContentAs<dynamic>());

            // Perform a SQL++ (N1QL) Query
            var queryResult = await cluster.QueryAsync<dynamic>(
                String.Format("SELECT name FROM `{0}` WHERE $1 IN interests", bucketName), 
                new QueryOptions().Parameter("African Swallows")
            );

            // Print each found Row
            await foreach (var row in queryResult)
            {
                Console.WriteLine(row);
            }
        }
    }
}
