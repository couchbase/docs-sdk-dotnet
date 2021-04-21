using System;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Couchbase.Examples.Logging.GenericHost
{
    public class Program
    {
        // tag::generic-host[]
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddFile("Logs/myapp-{Date}.txt", LogLevel.Debug);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>();
                    services.AddCouchbase(hostContext.Configuration.GetSection("couchbase"));
                    services.AddCouchbaseBucket<INamedBucketProvider>("default");
                });
        // end::generic-host[]
    }
}
