using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Couchbase.Examples.Logging.FullFramework
{
    // tag::full-log4netextensions[]
    public static class Log4netExtensions
    {
        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory, string log4NetConfigFile)
        {
            factory.AddProvider(new Log4NetProvider(log4NetConfigFile));
            return factory;
        }

        public static ILoggerFactory AddLog4Net(this ILoggerFactory factory)
        {
            factory.AddProvider(new Log4NetProvider("log4net.config"));
            return factory;
        }
    }
    // end::full-log4netextensions[]
}
