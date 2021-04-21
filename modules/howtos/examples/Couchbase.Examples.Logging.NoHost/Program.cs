using System;
using System.Threading.Tasks;
using Couchbase.Core.Diagnostics.Tracing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Couchbase.Examples.Logging.NoHost
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // tag::non-host[]
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => builder
                .AddFilter(level => level >= LogLevel.Debug)
            );
            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            loggerFactory.AddFile("Logs/myapp-{Date}.txt", LogLevel.Debug);

            var clusterOptions = new ClusterOptions().
                WithCredentials("Administrator", "password").
                WithLogging(loggerFactory);

            var cluster = Cluster.ConnectAsync("couchbase://10.112.211.101", clusterOptions).
                GetAwaiter().
                GetResult();
            // end::non-host[]

            Console.WriteLine("Hello World!");
        }
    }
}
