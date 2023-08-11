// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script UserManagementExample.csx
//

#r "nuget: CouchbaseNetClient, 3.2.0"

using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;

var cluster = await Cluster.ConnectAsync(
    "couchbase://your-ip",
    "Administrator", "password");
var bucket =  await cluster.BucketAsync("travel-sample");
var _collection = bucket.DefaultCollection();

var document = new {
    name = "Douglas Reynholm",
    // email = "douglas@reynholmindustries.com",
    addresses = new {
        billing = new {
            line1 = "123 Any Street",
            line2 = "Anytown",
            country = "United Kingdom"
        },
        delivery = new {
            line1 = "123 Any Street",
            line2 = "Anytown",
            country = "United Kingdom"
        },
    },
    purchases = new {
        complete = new[] { 339, 976, 442, 666 },
        abandoned = new[] { 157, 42, 999 },
    }
};
await _collection.UpsertAsync("customer123", document);

{
    Console.WriteLine("get:");
    // #tag::get[]
    var result = await _collection.LookupInAsync("customer123", specs =>
        specs.Get("addresses.delivery.country")
    );

    string country = result.ContentAs<string>(0);
    WriteLine(country);
    // #end::get[]
}

{
    Console.WriteLine("insert:");
    // #tag::insert[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.Insert("email", "dougr96@hotmail.com")
    );
    // #end::insert[]
}

{
    Console.WriteLine("exists:");
    // #tag::exists[]
    var result = await _collection.LookupInAsync("customer123", specs =>
        specs.Exists("addresses.delivery.does_not_exist")
    );

    bool exists = result.ContentAs<bool>(0);
    // #end::exists[]
}

{
    Console.WriteLine("combine:");
    // #tag::combine[]
    var result = await _collection.LookupInAsync("customer123", specs => {
        specs.Get("addresses.delivery.country");
        specs.Exists("addresses.delivery.does_not_exist");
    });

    string country = result.ContentAs<string>(0);
    bool exists = result.ContentAs<bool>(1);
    // #end::combine[]
}

{
    Console.WriteLine("upsert:");
    // #tag::upsert[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.Upsert("email", "dougr96@hotmail.com")
    );
    // #end::upsert[]
}

{
    Console.WriteLine("multi:");
    // #tag::multi[]
    await _collection.MutateInAsync("customer123", specs => {
        specs.Remove("addresses.billing");
        specs.Replace("email", "dougr96@hotmail.com");
    });
    // #end::multi[]
}

{
    Console.WriteLine("array-append:");
    // #tag::array-append[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.ArrayAppend("purchases.complete", new [] {777})
    );
    // purchases.complete is now [339, 976, 442, 666, 777]
    // #end::array-append[]
}

{
    Console.WriteLine("array-prepend:");
    // #tag::array-prepend[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.ArrayPrepend("purchases.abandoned", new [] {18})
    );
    // purchases.abandoned is now [18, 157, 49, 999]
    // #end::array-prepend[]
}

{
    Console.WriteLine("array-create:");
    // #tag::array-create[]
    await _collection.UpsertAsync("my_array", new object[] {});

    await _collection.MutateInAsync("my_array", specs =>
        specs.ArrayAppend("", new [] {"some element"})
    );
    // the document my_array is now ["some element"]
    // #end::array-create[]
}

await _collection.UpsertAsync("some_doc", 
    new {
        some = new {
            array = new object[] {} }});

{
    Console.WriteLine("array-upsert:");
    // #tag::array-upsert[]
    await _collection.MutateInAsync("some_doc", specs =>
        specs.ArrayAppend("some.array", new [] {"hello world"}, createPath: true)
    );
    // #end::array-upsert[]
}

{
    Console.WriteLine("array-unique:");
    // #tag::array-unique[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.ArrayAddUnique("purchases.complete", 95)
    );
    // #end::array-unique[]
}

{
    Console.WriteLine("array-insert:");
    // #tag::array-insert[]
    await _collection.MutateInAsync("some_doc", specs =>
        specs.ArrayInsert("some.array[1]", new[] {"cruel"})
    );
    // #end::array-insert[]
}

{
    Console.WriteLine("counter-inc:");
    // #tag::counter-inc[]
    var result = await _collection.MutateInAsync("customer123", specs =>
        specs.Increment("logins", 1)
    );

    // Counter operations return the updated count
    var count = result.ContentAs<long>(0);
    // #end::counter-inc[]
}

{
    Console.WriteLine("counter-dec:");
    // #tag::counter-dec[]
    await _collection.UpsertAsync("player432", new { gold = 1000 });

    var result = await _collection.MutateInAsync("player432", specs =>
        specs.Decrement("gold", 150)
    );

    var count = result.ContentAs<long>(0);
    // #end::counter-dec[]
}

{
    Console.WriteLine("create-path:");
    // #tag::create-path[]
    await _collection.MutateInAsync("customer123", specs =>
        specs.Upsert("level_0.level_1.foo.bar.phone", new { num = "311-555-0101", ext = 16 }, createPath: true)
    );
    // #end::create-path[]
}

async Task Concurrent() {
    Console.WriteLine("concurrent:");
    // #tag::concurrent[]
    // thread one
    await _collection.MutateInAsync("customer123",
        specs => specs.ArrayAppend("purchases.complete", 99));

    // thread two
    await _collection.MutateInAsync("customer123",
        specs => specs.ArrayAppend("purchases.abandoned", 101));
    // #end::concurrent[]
}

await Concurrent();
    
async Task CasAsync() {
    Console.WriteLine("cas:");
    // #tag::cas[]
    var player = await _collection.GetAsync("player432");
    await _collection.MutateInAsync("player432",
        specs => specs.Decrement("gold", 150),
        options => options.Cas(player.Cas)
    );
    // #end::cas[]
}

await CasAsync();
