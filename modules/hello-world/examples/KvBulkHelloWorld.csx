// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script KvBulkHelloWorld.csx
//

#r "nuget: CouchbaseNetClient, 3.2.5"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.KeyValue;
using Newtonsoft.Json.Linq;

await new KvBulkHelloWorld().ExampleAsync();

public class KvBulkHelloWorld
{
	public async Task ExampleAsync()
	{
		var cluster = await Cluster.ConnectAsync("couchbase://localhost", "Administrator", "password");
		var bucket = await cluster.BucketAsync("travel-sample");
		var tenantScope = await bucket.ScopeAsync("tenant_agent_00");
		var usersCollection = await tenantScope.CollectionAsync("users");

		// tag::kv-users[]
		var documents = new[]
		{
			new { id = "user_111", email = "tom_the_cat@gmail.com"},
			new { id = "user_222", email = "jerry_mouse@gmail.com"},
			new { id = "user_333", email = "mickey_mouse@gmail.com"}
		};
		// end::kv-users[]

		{
			Console.WriteLine("\n[kv-bulk-insert]");
			// tag::kv-bulk-insert[]
			// Collection of things that will complete in the future.
			var tasks = new List<Task<IMutationResult>>();

			// Create tasks to be executed concurrently.
			foreach (var document in documents)
			{
				Console.WriteLine($"Inserting document: {document.id}");
				var task = usersCollection.InsertAsync(document.id, document);
				tasks.Add(task);
			}

			// Wait until all of the tasks have completed.
			await Task.WhenAll(tasks);

			// Iterate task list to get results.
			foreach (var task in tasks)
				Console.WriteLine($"CAS: {task.Result.Cas}");
			// end::kv-bulk-insert[]
		}

		{
			Console.WriteLine("\n[kv-bulk-upsert]");
			// tag::kv-bulk-upsert[]
			var newDocuments = new[]
			{
				new { id = "user_111", email = "tom@gmail.com"},
				new { id = "user_222", email = "jerry@gmail.com"},
				new { id = "user_333", email = "mickey@gmail.com"}
			};

			// Collection of things that will complete in the future.
			var tasks = new List<Task<IMutationResult>>();

			// Create tasks to be executed concurrently.
			foreach (var newDocument in newDocuments)
			{
				Console.WriteLine($"Upserting document: {newDocument.id}");
				var task = usersCollection.UpsertAsync(newDocument.id, newDocument);
				tasks.Add(task);
			}

			// Wait until all of the tasks have completed.
			await Task.WhenAll(tasks);

			// Iterate task list to get results.
			foreach (var task in tasks)
				Console.WriteLine($"CAS: {task.Result.Cas}");
			// end::kv-bulk-upsert[]
		}

		{
			Console.WriteLine("\n[kv-bulk-get]");
			// tag::kv-bulk-get[]
			// Collection of things that will complete in the future.
			var tasks = new List<Task<IGetResult>>();

			// Create tasks to be executed concurrently.
			foreach (var document in documents)
			{
				Console.WriteLine($"Getting document: {document.id}");
				var task = usersCollection.GetAsync(document.id);
				tasks.Add(task);
			}

			// Wait until all of the tasks have completed.
			await Task.WhenAll(tasks);

			// Iterate task list to get results.
			foreach (var task in tasks)
				Console.WriteLine($"Document: {task.Result.ContentAs<dynamic>()}");
			// end::kv-bulk-get[]
		}

		{
			Console.WriteLine("\n[kv-bulk-remove]");
			// tag::kv-bulk-remove[]
			// Collection of things that will complete in the future.
			var tasks = new List<Task>();

			// Create tasks to be executed concurrently.
			foreach (var document in documents)
			{
				Console.WriteLine($"Removing document: {document.id}");
				var task = usersCollection.RemoveAsync(document.id);
				tasks.Add(task);
			}

			// Wait until all of the tasks have completed.
			// NOTE: RemoveAsync returns void, so no need to loop through each task.
			await Task.WhenAll(tasks);
			// end::kv-bulk-remove[]
		}
	}
}
