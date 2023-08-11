// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script ProvisioningResourcesViews.csx
//

#r "nuget: CouchbaseNetClient, 3.3.0"

using System;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Views;
using Couchbase.Management.Views;

await new ProvisioningResourcesViews().ExampleAsync();

public class ProvisioningResourcesViews
{
    public async Task ExampleAsync()
    {
        // tag::viewmgr[]
        var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
        var bucket = await cluster.BucketAsync("travel-sample");
        var viewMgr = bucket.ViewIndexes;
        // end::viewmgr[]

        {
            Console.WriteLine("[createView]");
            // tag::createView[]
            var views = new Dictionary<string, View>();
            views.Add(
                "by_country",
                new View{ Map = "function (doc, meta) { if (doc.type == 'landmark') { emit([doc.country, doc.city], null); } }" }
            );
            views.Add(
                "by_activity",
                new View{ Map="function (doc, meta) { if (doc.type == 'landmark') { emit([doc.country, doc.city], null); } }",
                Reduce="_count" }
            );

            var designDocument = new DesignDocument { Name = "landmarks", Views = views };
            await viewMgr.UpsertDesignDocumentAsync(designDocument, DesignDocumentNamespace.Development);
            // end::createView[]
        }

        {
            Console.WriteLine("[getView]");
            // tag::getView[]
            var designDocument = await viewMgr.GetDesignDocumentAsync("landmarks", DesignDocumentNamespace.Development);
            Console.WriteLine($"Design Document: {designDocument.Name}");
            // end::getView[]
        }


        {
            Console.WriteLine("[publishView]");
            // tag::publishView[]
            await viewMgr.PublishDesignDocumentAsync("landmarks");
            // end::publishView[]
        }

        {
            Console.WriteLine("[removeView]");
            // tag::removeView[]
            await viewMgr.DropDesignDocumentAsync("landmarks", DesignDocumentNamespace.Production);
            // end::removeView[]
        }

    }
}
