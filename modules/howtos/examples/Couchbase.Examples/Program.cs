using System;
using System.Threading.Tasks;

namespace Couchbase.Examples
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
            var bucket = await cluster.BucketAsync("travel-sample");
            var collection = bucket.DefaultCollection();

           /* var transcoding = new Transcoding(collection);
            await transcoding.RawJsonEncode();
            await transcoding.RawJsonDecode();
            await transcoding.String();
            await transcoding.CustomEncode();
            await transcoding.CustomDecode();
            await transcoding.MsgPackEncode();
            await transcoding.MsgPackDecode();*/

            await new KvOperations().ExecuteAsync();
            Console.WriteLine("Hit Enter to continue.");
            Console.Read();
            await new CollectionManager().ExecuteAsync();
        }
    }
}
