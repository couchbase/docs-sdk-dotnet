using System;
using System.Threading.Tasks;
using Couchbase.Core.Exceptions;
using Couchbase.Management.Query;
using Couchbase.Query;
using Microsoft.Extensions.Logging;

namespace Couchbase.Net.DevGuide
{
    public class QueryConsistency : ConnectionBase
    {
        public override async Task ExecuteAsync()
        {
            //Connect to Couchbase
            await ConnectAsync().ConfigureAwait(false);

            var key = "DotNetDevguideExampleQueryConsistency";

            try
            {
                await Cluster.QueryIndexes
                    .CreatePrimaryIndexAsync(Bucket.Name, new CreatePrimaryQueryIndexOptions().IgnoreIfExists(true))
                    .ConfigureAwait(false);
            }
            catch (InternalServerFailureException)
            {
                Console.WriteLine("Primary index already exists!");
            }

            var random = new Random();
            var randomNumber = random.Next(10000000);

            //prepare the random user
            var user = new
            {
                name = new[] {"Brass", "Doorknob"},
                email = "brass.doorknob@juno.com",
                random = randomNumber
            };

            //upsert it with the corresponding random key
            await Bucket.DefaultCollection().UpsertAsync<dynamic>(key, user).ConfigureAwait(false);

            Logger.LogInformation("Expecting random: " + randomNumber);
            var result = await Cluster.QueryAsync<dynamic>(
                    "select name, email, random, META(default).id from default where $1 in name",
                    new QueryOptions().ScanConsistency(QueryScanConsistency.RequestPlus).Parameter("Brass"))
                .ConfigureAwait(false);

            if (!result.MetaData.Status.Equals(QueryStatus.Success))
                Console.WriteLine("No result/errors: " + result.MetaData.Warnings);

            await foreach (var row in result)
            {
                var rowRandom = row.random;
                string rowId = row.id;

                Console.WriteLine($"Doc Id: {rowId}, Name:  {row.name} , Email: {row.email}, Random: {rowRandom}");

                Console.WriteLine(rowRandom == randomNumber
                    ? "!!! Found our newly inserted document !!!"
                    : "Found a different random value : {rowRandom}");

                await Bucket.DefaultCollection().RemoveAsync(rowId).ConfigureAwait(false);
            }
        }

        private new static async Task Main(string[] args)
        {
            await new QueryConsistency().ExecuteAsync().ConfigureAwait(false);
        }
    }
}