
using Microsoft.Extensions.Logging;
using Couchbase;
using System;
using System.Threading.Tasks;
using NLog;
using NLog.Extensions.Logging;

namespace LoggingExample
{
    public  class Program
    {
      
      // #tag::log[]
        static async Task Main(string[] args)
        {
           ILoggerFactory loggerFactory = new LoggerFactory();

            loggerFactory.AddNLog(new NLogProviderOptions
            {
                CaptureMessageTemplates = true,
                CaptureMessageProperties = true
            });
         
             LogManager.LoadConfiguration("nlog.config");

            var options = new ClusterOptions()
            {
                Logging = loggerFactory,
                UserName = "Administrator",
                Password = "password",
                ConnectionString = "http://localhost"
            };
           
           var cluster = await Cluster.ConnectAsync(options);
           var bucket = await cluster.BucketAsync("beer-sample");
           var collection = bucket.DefaultCollection();

           await collection.UpsertAsync<string>("beer-sample-101", "logging example 101");

           var returnVal = await collection.GetAsync("beer-sample-101");        

          
            Console.WriteLine(returnVal.ContentAs<string>());

            Console.Read();
        }
       // #end::log[]
    }
}

