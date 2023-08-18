// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script IndexHelloWorld.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Query;
using Couchbase.Core.Exceptions;

await new IndexHelloWorld().ExampleAsync();

public class IndexHelloWorld
{
	public async Task ExampleAsync()
	{
		var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");

		{
			Console.WriteLine("[primary]");
			// NOTE: 
			// * Bucket names containing a `-` need to be escaped, see https://issues.couchbase.com/browse/NCBC-2955.
			// * The `IgnoreIfExists` option is currently not working as expected, see https://issues.couchbase.com/browse/NCBC-2647.
			try
			{
				// tag::primary[]
				await cluster.QueryIndexes.CreatePrimaryIndexAsync(
					"`travel-sample`",
					options => options.IgnoreIfExists(true)
				);
				// end::primary[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}
			Console.WriteLine("Index creation complete");
		}

		{
			Console.WriteLine("\n[named-primary]");
			try
			{
			// tag::named-primary[]
			await cluster.QueryIndexes.CreatePrimaryIndexAsync(
				"`travel-sample`",
				options => options.IndexName("named_primary_index")
			);
			// end::named-primary[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}
			Console.WriteLine("Named primary index creation complete");
		}

		{
			Console.WriteLine("\n[secondary]");
			try
			{
			// tag::secondary[]
			await cluster.QueryIndexes.CreateIndexAsync(
				"`travel-sample`",
				"index_name",
				new[] { "name" }
			);
			// end::secondary[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}
			Console.WriteLine("Index creation complete");
		}

		{
			Console.WriteLine("\n[composite]");
			try
			{
			// tag::composite[]
			await cluster.QueryIndexes.CreateIndexAsync(
				"`travel-sample`",
				"index_travel_info",
				new[] { "name", "id", "icao", "iata" }
			);
			// end::composite[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}
			Console.WriteLine("Index creation complete");
		}

		{
			Console.WriteLine("\n[drop-primary]");
			try
			{
			// tag::drop-primary[]
			await cluster.QueryIndexes.DropPrimaryIndexAsync("`travel-sample`");
			// end::drop-primary[]
			}
			catch (IndexNotFoundException)
			{
				Console.WriteLine("Index not found");
			}
			Console.WriteLine("Primary index deleted successfully");
		}

		{
			Console.WriteLine("\n[drop-named-primary]");
			try
			{
			// tag::drop-named-primary[]
			await cluster.QueryIndexes.DropPrimaryIndexAsync(
				"`travel-sample`",
				options => options.IndexName("named_primary_index")
			);
			// end::drop-named-primary[]
			}
			catch (IndexNotFoundException)
			{
				Console.WriteLine("Index not found");
			}
			Console.WriteLine("Named primary index deleted successfully");
		}

		{
			Console.WriteLine("\n[drop-secondary]");
			try
			{
			// tag::drop-secondary[]
			await cluster.QueryIndexes.DropIndexAsync("`travel-sample`", "index_name");
			// end::drop-secondary[]
			}
			catch (IndexNotFoundException)
			{
				Console.WriteLine("Index not found");
			}
			Console.WriteLine("Index deleted successfully");
		}

		{
			Console.WriteLine("\n[defer-create]");
			try
			{
			// tag::defer-create-primary[]
			await cluster.QueryIndexes.CreatePrimaryIndexAsync(
				"`travel-sample`",
				 options => options.Deferred(true)
			);
			// end::defer-create-primary[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}

			try
			{
			// tag::defer-create-secondary[]
			await cluster.QueryIndexes.CreateIndexAsync(
				"`travel-sample`",
				"idx_name_email",
				new[] { "name", "email" },
				options => options.Deferred(true)
			);
			// end::defer-create-secondary[]
			}
			catch (IndexExistsException)
			{
				Console.WriteLine("Index already exists");
			}
			Console.WriteLine("Created deferred indexes");
		}

		{
			Console.WriteLine("\n[defer-build]");
			// tag::defer-build[]
			// Start building any deferred indexes which were previously created.
			await cluster.QueryIndexes.BuildDeferredIndexesAsync("`travel-sample`");

			// Wait for the deferred indexes to be ready for use.
			await cluster.QueryIndexes.WatchIndexesAsync(
				"`travel-sample`",
				new[] { "idx_name_email" },
				options => options.WatchPrimary(true)
			);
			// end::defer-build[]
			Console.WriteLine("Deferred indexes ready");
		}
	}
}
