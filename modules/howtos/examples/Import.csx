// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script Import.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"
#r "nuget: CsvHelper, 27.2.1"

using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;

// tag::csv-tsv-import[]
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
// end::csv-tsv-import[]

// tag::json-jsonl-import[]
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
// end::json-jsonl-import[]


using System.Runtime.CompilerServices;

public static string GetScriptFolder([CallerFilePath] string path = null) => Path.GetDirectoryName(path);
Directory.SetCurrentDirectory(GetScriptFolder());

// tag::connect[]
var cluster = await Cluster.ConnectAsync(
    "couchbase://your-ip",
    "Administrator", "password");
var bucket =  await cluster.BucketAsync("travel-sample");
var scope = await bucket.ScopeAsync("inventory");
var _collection = await scope.CollectionAsync("airline");
// end::connect[]

// tag::importCSV[]
public async Task importCSV(string filename)
{
    using (var reader = new StreamReader(filename))
    using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
    {
        var records = csv.GetRecords<dynamic>();
        foreach (dynamic record in records) {
            await upsertDocument(record);
        }
    }
}
// end::importCSV[]
await importCSV("import.csv");

// tag::importTSV[]
public async Task importTSV(string filename)
{
    using (var reader = new StreamReader("import.tsv"))
    using (var tsv = new CsvReader(reader,
        new CsvConfiguration(CultureInfo.InvariantCulture)
            { Delimiter = "\t" }))
    {
        var records = tsv.GetRecords<dynamic>();
        foreach (dynamic record in records) {
            await upsertDocument(record);
        }
    }
}
// end::importTSV[]
await importTSV("import.tsv");

// tag::importJSON[]
public async Task importJSON(string filename)
{
    using (var reader = new StreamReader("import.json"))
    {
        var jsonReader = new JsonTextReader(reader);
        JArray arr = (JArray)JToken.ReadFrom(jsonReader);
        
        foreach (JObject record in arr)
        {
            await upsertDocument(record);
        }
    }
}
// end::importJSON[]
await importJSON("import.json");

// tag::importJSONL[]
public async Task importJSONL(string filename)
{
    using (var reader = new StreamReader("import.jsonl"))
    {
        var jsonlReader = new JsonTextReader(reader)
        {
            SupportMultipleContent = true
        };
        while (jsonlReader.Read())
        {
            var record = (JObject)JToken.ReadFrom(jsonlReader);
            await upsertDocument(record);
        }
    }
}
// end::importJSONL[]
await importJSONL("import.jsonl");


// tag::upsertDocument[]
// CsvHelper emits `dynamic` records
public async Task upsertDocument(dynamic record) {
    // define the key
    string key = record.type + "_" + record.id;

    // do any additional processing
    record.importer = ".NET SDK";

    // upsert the document
    await _collection.UpsertAsync(key, record);

    // any required logging
    Console.WriteLine(key);
}

// Newtonsoft.Json.Linq emits `JObjects`
public async Task upsertDocument(JObject record) {
    // define the key
    string key = record["type"] + "_" + record["id"];

    // do any additional processing
    record["importer"] = ".NET SDK";
    
    // upsert the document
    await _collection.UpsertAsync(key, record);
    
    // any required logging
    Console.WriteLine(key);
}
// end::upsertDocument[]
