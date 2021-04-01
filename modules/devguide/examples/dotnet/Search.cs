using System;
using System.Threading.Tasks;
using Couchbase.Search;
using Couchbase.Search.Queries.Simple;

namespace Couchbase.Net.DevGuide
{
    public class Search : ConnectionBase
    {
        internal static string IndexName = "travel-sample-index";

        public override async Task ExecuteAsync()
        {
            Console.WriteLine("Starting Search example");
            Console.WriteLine(Cluster);
            var results = await Cluster.SearchQueryAsync(IndexName,
                new MatchQuery("inn"),
                new SearchOptions().Facets(
                    new TermFacet("termfacet", "name", 1),
                    new DateRangeFacet("daterangefacet", "thefield", 10).AddRange(DateTime.Now, DateTime.Now.AddDays(1)),
                    new NumericRangeFacet("numericrangefacet", "thefield", 2).AddRange(2.2f, 3.5f)
                )
            ).ConfigureAwait(false);

            foreach (var row in results)
            {
                Console.WriteLine(row);
            }
            Console.WriteLine("Ended Search example");

        }

        private new static async Task Main(string[] args)
        {
            await new Search().ExecuteAsync().ConfigureAwait(false);
        }
    }
}
