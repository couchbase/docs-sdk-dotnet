using System;
using System.Threading;
using System.Threading.Tasks;
using Couchbase.KeyValue;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Couchbase.Net.DevGuide
{
    internal class Program2
    {
        private ICluster cluster;
        private IBucket bucket;
        private ICouchbaseCollection collection;
        private ILoggerFactory loggerFactory;

        private static void Main2(string[] args)
        {
            var inst = new Program2();

            try
            {
                inst.Initialize().Wait();
                inst.RunTest().Wait();
                inst.FreeUpStuff();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an exception: {ex}");
            }

            Console.WriteLine("Done.");
        }

        private async Task RunTest()
        {
            var getResult = await collection.GetAsync("airline_5209").ConfigureAwait(false);
            var docContent = getResult.ContentAs<dynamic>();
            Console.WriteLine($"The airline is {docContent.name}.");

            try
            {
                var queryResult = await cluster.QueryAsync<dynamic>("SELECT * FROM `travel-sample` LIMIT 10",
                    parameter => { parameter.Parameter("type", "airline"); }).ConfigureAwait(false);

                await foreach (var o in queryResult.ConfigureAwait(false))
                    Console.WriteLine(JsonConvert.SerializeObject(o, Formatting.None));
            }
            catch (CouchbaseException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void FreeUpStuff()
        {
            bucket.Dispose();
            cluster.Dispose();
        }

        private async Task Initialize()
        {
            /* Logging here used the following dependencies in the associated .csproj file
             *     <PackageReference Include="Microsoft.Extensions.Logging.Console" Version="3.1.3" />
             *     <PackageReference Include="Serilog.Extensions.Logging.File" Version="2.0.0-dev-00039" />
             */

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                    .AddFilter(level => level >= LogLevel.Information) // change to Debug if trying to diagnose
            );
            loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            loggerFactory.AddFile("Logs/myapp-{Date}.txt"); // change to Debug if trying to diagnose


            /* Note: in the configuration below, EnableTls is required as Couchbase Capella is
             * always TLS. Also, the IgnoreRemoteCertificateNameMismatch will trust the asserted
             * CA Cert by Couchbase Capella. There is a known issue that prevents programmatically
             * adding the cert, so either this or adding it to the platform is required.
             */
            var config = new ClusterOptions
            {
                UserName = "username",
                Password = "password",
                IgnoreRemoteCertificateNameMismatch = true, // workaround for CA Cert
                EnableTls = true
            }.WithLogging(loggerFactory);

            cluster = await Cluster.ConnectAsync(
                "couchbases://6b115491-a379-4bc3-bedb-780c5fbdf8a8.dp.cloud.couchbase.com", config);

            Thread.Sleep(500);

            bucket = await cluster.BucketAsync("travel-sample");
            collection = bucket.DefaultCollection();
        }
    }
}
