// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script KvHelloWorldScoped.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Newtonsoft.Json.Linq;

await new KvHelloWorldScoped().ExampleAsync();

public class KvHelloWorldScoped
{
	public async Task ExampleAsync()
	{
		var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
		var bucket = await cluster.BucketAsync("travel-sample");
		var inventoryScope = await bucket.ScopeAsync("inventory");
		var hotelCollection = await inventoryScope.CollectionAsync("hotel");

		{
			Console.WriteLine("[kv-insert]");
			// tag::kv-insert[]
			// Create a document object.
			var document = new
			{
				id = 123,
				name = "Medway Youth Hostel ",
				address = "Capstone Road, ME7 3JE",
				url = "http://www.yha.org.uk",
				geo = new
				{
					lat = 51.35785,
					lon = 0.55818,
					accuracy = "RANGE_INTERPOLATED"
				},
				country = "United Kingdom",
				city = "Medway",
				state = (string)null,
				reviews = new[]
				{
					new {
						content = "This was our 2nd trip here and we enjoyed it more than last year.",
						author = "Ozella Sipes",
						date = DateTime.UtcNow
					}
				},
				vacancy = true,
				description = "40 bed summer hostel about 3 miles from Gillingham."
			};

			// Insert the document in the hotel collection.
			var insertResult = await hotelCollection.InsertAsync("hotel-123", document);

			// Print the result's CAS metadata to the console.
			Console.WriteLine($"Cas: {insertResult.Cas}");
			// end::kv-insert[]
		}

		{
			Console.WriteLine("\n[kv-insert-with-opt]");
			// tag::kv-insert-with-opts[]
			var document = new
			{
				id = 456,
				title = "Ardèche",
				name = "La Pradella",
				address = "rue du village, 07290 Preaux, France",
				phone = "+33 4 75 32 08 52",
				url = "http://www.lapradella.fr",
				country = "France",
				city = "Preaux",
				state = "Rhône-Alpes",
				vacancy = false
			};

			// Insert the document with an expiry time option of 60 seconds.
			var insertResult = await hotelCollection.InsertAsync("hotel-456", document, options =>
			{
				options.Expiry(TimeSpan.FromSeconds(60));
			});

			// Print the result's CAS metadata to the console.
			Console.WriteLine($"CAS: {insertResult.Cas}");
			// end::kv-insert-with-opts[]
		}

		{
			Console.WriteLine("\n[kv-get]");
			// tag::kv-get[] 
			var getResult = await hotelCollection.GetAsync("hotel-123");

			// Print some result metadata to the console.
			Console.WriteLine($"CAS: {getResult.Cas}");
			Console.WriteLine($"Data: {getResult.ContentAs<JObject>()}");
			// end::kv-get[] 
		}

		{
			Console.WriteLine("\n[kv-get-with-opts]");
			// tag::kv-get-with-opts[] 
			var getResult = await hotelCollection.GetAsync("hotel-456", options =>
			{
				options.Expiry();
			});

			// Print some result metadata to the console.
			Console.WriteLine($"CAS: {getResult.Cas}");
			Console.WriteLine($"Data: {getResult.ContentAs<JObject>()}");
			Console.WriteLine($"Expiry: {getResult.ExpiryTime}");
			// end::kv-get-with-opts[] 
		}

		{
			Console.WriteLine("\n[kv-get-subdoc]");
			// tag::kv-get-subdoc[] 
			var lookupInResult = await hotelCollection.LookupInAsync("hotel-123",
					specs => specs.Get("geo")
			);

			Console.WriteLine($"CAS: {lookupInResult.Cas}");
			Console.WriteLine($"Geo: {lookupInResult.ContentAs<JObject>(0)}");
			// end::kv-get-subdoc[] 
		}

		{
			Console.WriteLine("\n[kv-update-replace]");
			// tag::kv-update-replace[]
			// Fetch an existing hotel document.
			var getResult = await hotelCollection.GetAsync("hotel-123");
			var existingDoc = getResult.ContentAs<JObject>();

			// Get the current CAS value.
			var currentCas = getResult.Cas;
			Console.WriteLine($"Current Cas: {currentCas}");

			// Add a new review to the reviews array.
			var reviews = (JArray)existingDoc["reviews"];
			reviews.Add(new JObject(
				new JProperty("content", "This hotel was cozy, conveniently located and clean."),
				new JProperty("author", "Carmella O'Keefe"),
				new JProperty("date", DateTime.UtcNow))
			);

			// Update the document with new data and pass the current CAS value. 
			var replaceResult = await hotelCollection.ReplaceAsync("hotel-123", existingDoc, options =>
			{
				options.Cas(currentCas);
			});

			// Print the new CAS value.
			Console.WriteLine($"New Cas: {replaceResult.Cas}");
			// end::kv-update-replace[]
		}

		{
			Console.WriteLine("\n[kv-update-upsert]");
			// Create a document object.
			var document = new
			{
				id = 123,
				name = "Medway Youth Hostel ",
				address = "Capstone Road, ME7 3JE",
				url = "http://www.yha.org.uk",
				country = "United Kingdom",
				city = "Medway",
				state = (string)null,
				vacancy = true,
				description = "40 bed summer hostel about 3 miles from Gillingham."
			};

			// tag::kv-update-upsert[]
			// Update or create a document in the hotel collection.
			var upsertResult = await hotelCollection.UpsertAsync("hotel-123", document);

			// Print the result's CAS metadata to the console.
			Console.WriteLine($"Cas: {upsertResult.Cas}");
			// end::kv-update-upsert[]
		}

		{
			Console.WriteLine("\n[kv-update-subdoc]");
			// tag::kv-update-subdoc[]
			var mutateInResult = await hotelCollection.MutateInAsync("hotel-123",
				specs => specs.Upsert("pets_ok", true)
			);
			Console.WriteLine($"Cas: {mutateInResult.Cas}");
			// end::kv-update-subdoc[]
		}

		{
			Console.WriteLine("\n[kv-remove-subdoc]");
			// tag::kv-remove-subdoc[]
			var mutateInResult = await hotelCollection.MutateInAsync("hotel-123",
				specs => specs.Remove("url")
			);
			Console.WriteLine($"Cas: {mutateInResult.Cas}");
			// end::kv-remove-subdoc[]
		}

		{
			Console.WriteLine("\n[kv-remove]");
			// tag::kv-remove[]
			await hotelCollection.RemoveAsync("hotel-123");
			// end::kv-remove[]
		}
	}
}
