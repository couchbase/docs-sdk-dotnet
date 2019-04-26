using System.Threading.Tasks;
using Couchbase;

namespace ConsoleApp1
{
    class Program
    {
        private static ICollection _collection;

        static async Task Main(string[] args)
        {
            var config = new Configuration()
                .WithServers("127.0.0.1")
                .WithCredentials("Administrator", "password")
                .WithBucket("bucket-name");

            var cluster = new Cluster(config);
            var bucket = await cluster.Bucket("default");
            _collection = await bucket.DefaultCollection;
        }

        async Task Get()
        {
// #tag::get[]
            var result = await _collection.LookupIn("customer123", ops =>
                ops.Get("addresses.delivery.country")
            );

            string country = result.ContentAs<string>(0);
// #end::get[]
        }

        async Task Exists() {
// #tag::exists[]
            var result = await _collection.LookupIn("customer123", ops =>
                ops.Exists("addresses.delivery.does_not_exist")
            );

            bool exists = result.ContentAs<bool>(0);
// #end::exists[]
        }

        async Task Combine() {
// #tag::combine[]
            var result = await _collection.LookupIn("customer123", ops => {
                ops.Get("addresses.delivery.country");
                ops.Exists("addresses.delivery.does_not_exist");
            });

            string country = result.ContentAs<string>(0);
            bool exists = result.ContentAs<bool>(1);
// #end::combine[]
        }

        async Task Upsert() {
// #tag::upsert[]
            await _collection.MutateIn("customer123", ops =>
                ops.Upsert("email", "dougr96@hotmail.com")
            );
 // #end::upsert[]
        }

        async Task Insert() {
// #tag::insert[]
            await _collection.MutateIn("customer123", ops =>
                ops.Insert("email", "dougr96@hotmail.com")
            );
// #end::insert[]
        }

        async Task Multi() {
// #tag::multi[]
            await _collection.MutateIn("customer123", ops => {
                ops.Remove("addresses.billing");
                ops.Replace("email", "dougr96@hotmail.com");
            });
// #end::multi[]
        }

        async Task ArrayAppend() {
// #tag::array-append[]
            await _collection.MutateIn("customer123", ops =>
                ops.ArrayAppend("purchases.complete", new [] {777})
            );
            // purchases.complete is now [339, 976, 442, 666, 777]
// #end::array-append[]
    }

        async Task ArrayPrepend() {
// #tag::array-prepend[]
            await _collection.MutateIn("customer123", ops =>
                ops.ArrayPrepend("purchases.abandoned", new [] {18})
            );
            // purchases.abandoned is now [18, 157, 49, 999]
// #end::array-prepend[]
        }

        async Task CreateAndPopulateArrays()
        {
// #tag::array-create[]
            await _collection.Upsert("my_array", new object[] {});

            await _collection.MutateIn("my_array", ops =>
                ops.ArrayAppend("", new [] {"some element"})
            );
        	// the document my_array is now ["some element"]
// #end::array-create[]
        }

        async Task ArrayCreate() {
// #tag::array-upsert[]
            await _collection.MutateIn("some_doc", ops =>
                ops.ArrayAppend("some.array", new [] {"hello world"}, createPath: true)
            );
// #end::array-upsert[]
        }

        async Task ArrayUnique() {
// #tag::array-unique[]
            await _collection.MutateIn("customer123", ops =>
                ops.ArrayAddUnique("purchases.complete", 95)
            );
 // #end::array-unique[]
        }

        async Task ArrayInsert() {
// #tag::array-insert[]
            await _collection.MutateIn("some_doc", ops =>
                ops.ArrayInsert("foo.bar[1]", new[] {"cruel"})
            );
// #end::array-insert[]
        }

        async Task CounterInc() {
// #tag::counter-inc[]
            var result = await _collection.MutateIn("customer123", ops =>
                ops.Increment("logins", 1)
            );

            // Counter operations return the updated count
            var count = result.ContentAs<long>(0);
// #end::counter-inc[]
        }

        async Task CounterDec() {
// #tag::counter-dec[]
            await _collection.Upsert("player432", new { gold = 1000 });

            var result = await _collection.MutateIn("player432", ops =>
                ops.Decrement("gold", 150)
            );

            var count = result.ContentAs<long>(0);
// #end::counter-dec[]
        }

        async Task CreatePath() {
// #tag::create-path[]
            await _collection.MutateIn("customer123", ops =>
                ops.Upsert("level_0.level_1.foo.bar.phone", new { num = "311-555-0101", ext = 16 }, createPath: true)
            );
// #end::create-path[]
        }

        async Task Cas() {
// #tag::cas[]
            var player = await _collection.Get("player432");
            await _collection.MutateIn("player432",
                ops => ops.Decrement("gold", 150),
                options => options.WithCas(player.Cas)
            );
// #end::cas[]
        }
    }
}
