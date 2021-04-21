using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Couchbase;

namespace Couchbase.Examples.Logging.FullFramework
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // tag::full-framework[]
            ILoggerFactory factory = new LoggerFactory();
            factory.AddLog4Net("log4net.config");

            var ipAddressList = new List<string> { "10.112.205.101" };
            var config = new ClusterOptions()
                .WithConnectionString(string.Concat("http://", string.Join(", ", ipAddressList)))
                .WithCredentials("Administrator", "password")
                .WithBuckets("default")
                .WithLogging(factory); //<= Need to add the ILoggerFactory via DI

            config.KvConnectTimeout = TimeSpan.FromMilliseconds(12000);

            var cluster = await Cluster.ConnectAsync(config);
            var bucket = await cluster.BucketAsync("default");
            Console.ReadKey();
            // end::full-framework[]
        }
    }
}
