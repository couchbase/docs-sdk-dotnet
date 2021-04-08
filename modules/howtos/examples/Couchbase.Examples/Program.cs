using System;
using System.Threading.Tasks;

namespace Couchbase.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            await new KvOperations().ExecuteAsync();
        }
    }
}
