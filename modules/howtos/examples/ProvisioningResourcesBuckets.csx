// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script ProvisioningResourcesBuckets.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

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
                    RamQuotaMB = 100,
                    NumReplicas = 0,
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
            settings.RamQuotaMB = 100;
            settings.FlushEnabled = true;
            settings.ConflictResolutionType = null;

            await bucketMgr.UpdateBucketAsync(settings);
            // end::updateBucket[]
        }
        // Flushing immediately results in an "unexpected error"
        await Task.Delay(TimeSpan.FromSeconds(1));
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
