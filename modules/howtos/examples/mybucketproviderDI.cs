using Couchbase.Extensions.DependencyInjection;

namespace Couchbase.Docs.Examples.Howtos.DependencyInjection;

// tag::mybucketprovider[]
public interface IMyBucketProvider : INamedBucketProvider
{
}
// end::mybucketprovider[]
