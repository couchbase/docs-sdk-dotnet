// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script ProvisioningResourcesBuckets.csx
//

#r "nuget: CouchbaseNetClient, 3.3.0"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Management.Buckets;

await new ProvisioningResourcesBuckets().ExampleAsync();

public class ProvisioningResourcesBuckets
{
    public async Task ExampleAsync()
    {
        // tag::creatingBucketMgr[]
        var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
        var bucketMgr = cluster.Buckets;
        // end::creatingBucketMgr[]

        {
            Console.WriteLine("[createBucket]");
            // tag::createBucket[]
            await bucketMgr.CreateBucketAsync(
                new BucketSettings{
                    Name = "hello",
                    FlushEnabled = false,
                    ReplicaIndexes = true,
                    RamQuotaMB = 150,
                    NumReplicas = 1,
                    BucketType = BucketType.Couchbase,
                    ConflictResolutionType = ConflictResolutionType.SequenceNumber
                }
            );
            // end::createBucket[]
        }

        {
            Console.WriteLine("[updateBucket]");
            // tag::updateBucket[]
            var settings = await bucketMgr.GetBucketAsync("hello");
            settings.FlushEnabled = true;

            await bucketMgr.UpdateBucketAsync(settings);
            // end::updateBucket[]
        }

        {
            Console.WriteLine("[flushBucket]");
            // tag::flushBucket[]
            await bucketMgr.FlushBucketAsync("hello");
            // end::flushBucket[]
        }

        {
            Console.WriteLine("[removeBucket]");
            // tag::removeBucket[]
            await bucketMgr.DropBucketAsync("hello");
            // end::removeBucket[]
        }
    }
}
