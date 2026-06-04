using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace Couchbase.Examples.Logging.FullFramework
{
    // tag::full-log4netprovider[]
    public class Log4NetProvider : ILoggerProvider
    {
        private readonly string _log4NetConfigFile;
        private readonly ConcurrentDictionary<string, Log4NetLogger> _loggers =
            new ConcurrentDictionary<string, Log4NetLogger>();
        public Log4NetProvider(string log4NetConfigFile)
        {
            _log4NetConfigFile = log4NetConfigFile;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, CreateLoggerImplementation);
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
        private Log4NetLogger CreateLoggerImplementation(string name)
        {
            return new Log4NetLogger(name, ParseLog4NetConfigFile(_log4NetConfigFile));
        }

        private static XmlElement ParseLog4NetConfigFile(string filename)
        {
            var log4NetConfig = new XmlDocument();
            log4NetConfig.Load(File.OpenRead(filename));
            return log4NetConfig["log4net"];
        }
    }
    // end::full-log4netprovider[]
}
