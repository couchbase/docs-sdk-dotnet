using System.Threading.Tasks;
using Couchbase;

namespace ConsoleApp1
{
    class Program
    {
        private static ICollection _collection;

        static async Task Main(string[] args)
        {
            var options = new ClusterOptions()
                .WithServers("127.0.0.1")
                .WithCredentials("Administrator", "password")
                .WithBucket("bucket-name");

            var cluster = new Cluster(options);
            var bucket = await cluster.BucketAsync("default");
            _collection = await bucket.DefaultCollection();
        }

        async Task GetAsync()
        {
// #tag::get[]
            var result = await _collection.LookupInAsync("customer123", specs =>
                specs.Get("addresses.delivery.country")
            );

            string country = result.ContentAs<string>(0);
// #end::get[]
        }

        async Task ExistsAsync() {
// #tag::exists[]
            var result = await _collection.LookupInAsync("customer123", specs =>
                specs.Exists("addresses.delivery.does_not_exist")
            );

            bool exists = result.ContentAs<bool>(0);
// #end::exists[]
        }

        async Task CombineAsync() {
// #tag::combine[]
            var result = await _collection.LookupInAsync("customer123", specs => {
                specs.Get("addresses.delivery.country");
                specs.Exists("addresses.delivery.does_not_exist");
            });

            string country = result.ContentAs<string>(0);
            bool exists = result.ContentAs<bool>(1);
// #end::combine[]
        }

        async Task UpsertAsync() {
// #tag::upsert[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.Upsert("email", "dougr96@hotmail.com")
            );
 // #end::upsert[]
        }

        async Task InsertAsync() {
// #tag::insert[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.Insert("email", "dougr96@hotmail.com")
            );
// #end::insert[]
        }

        async Task MultiAsync() {
// #tag::multi[]
            await _collection.MutateInAsync("customer123", specs => {
                specs.Remove("addresses.billing");
                specs.Replace("email", "dougr96@hotmail.com");
            });
// #end::multi[]
        }

        async Task ArrayAppendAsync() {
// #tag::array-append[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.ArrayAppend("purchases.complete", new [] {777})
            );
            // purchases.complete is now [339, 976, 442, 666, 777]
// #end::array-append[]
    }

        async Task ArrayPrependAsync() {
// #tag::array-prepend[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.ArrayPrepend("purchases.abandoned", new [] {18})
            );
            // purchases.abandoned is now [18, 157, 49, 999]
// #end::array-prepend[]
        }

        async Task CreateAndPopulateArraysAsync()
        {
// #tag::array-create[]
            await _collection.UpsertAsync("my_array", new object[] {});

            await _collection.MutateInAsync("my_array", specs =>
                specs.ArrayAppend("", new [] {"some element"})
            );
        	// the document my_array is now ["some element"]
// #end::array-create[]
        }

        async Task ArrayCreateAsync() {
// #tag::array-upsert[]
            await _collection.MutateInAsync("some_doc", specs =>
                specs.ArrayAppend("some.array", new [] {"hello world"}, createPath: true)
            );
// #end::array-upsert[]
        }

        async Task ArrayUniqueAsync() {
// #tag::array-unique[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.ArrayAddUnique("purchases.complete", 95)
            );
 // #end::array-unique[]
        }

        async Task ArrayInsertAsync() {
// #tag::array-insert[]
            await _collection.MutateInAsync("some_doc", specs =>
                specs.ArrayInsert("foo.bar[1]", new[] {"cruel"})
            );
// #end::array-insert[]
        }

        async Task CounterIncAsync() {
// #tag::counter-inc[]
            var result = await _collection.MutateInAsync("customer123", specs =>
                specs.Increment("logins", 1)
            );

            // Counter operations return the updated count
            var count = result.ContentAs<long>(0);
// #end::counter-inc[]
        }

        async Task CounterDecAsync() {
// #tag::counter-dec[]
            await _collection.Upsert("player432", new { gold = 1000 });

            var result = await _collection.MutateInAsync("player432", specs =>
                specs.Decrement("gold", 150)
            );

            var count = result.ContentAs<long>(0);
// #end::counter-dec[]
        }

        async Task CreatePathAsync() {
// #tag::create-path[]
            await _collection.MutateInAsync("customer123", specs =>
                specs.Upsert("level_0.level_1.foo.bar.phone", new { num = "311-555-0101", ext = 16 }, createPath: true)
            );
// #end::create-path[]
        }

// #tag::concurrent
// thread one
await _collection.MutateInAsync("customer123", specs =>{
   specs.ArrayAppend("purchases.complete", 99)
});

// thread two
await _collection.MutateInAsync("customer123", specs =>
{
   specs.ArrayAppend("purchases.abandoned", 101)
});
// #end::concurrent
        
        async Task CasAsync() {
// #tag::cas[]
            var player = await _collection.GetAsync("player432");
            await _collection.MutateInAsync("player432",
                specs => specs.Decrement("gold", 150),
                options => options.WithCas(player.Cas)
            );
// #end::cas[]
        }
    }
}
