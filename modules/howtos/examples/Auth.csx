// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script Auth.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"

using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.IO.Authentication.X509;

await new Auth().ExecuteAsync();
public class Auth
{
    public async Task ExecuteAsync()
    {
        {
            // tag::basic[]
            try
            {
                var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
                // use the cluster
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to open cluster: {e}");
            }

            // end::basic]
        }
        {
            // tag::auth[]
            var options = new ClusterOptions();
            
            options.WithX509CertificateFactory(CertificateFactory.GetCertificatesFromStore(
                    new CertificateStoreSearchCriteria
                    {
                        FindValue = "value",
                        X509FindType = X509FindType.FindBySubjectName,
                        StoreLocation = StoreLocation.CurrentUser,
                        StoreName = StoreName.CertificateAuthority
                    }));
            options.WithConnectionString("couchbase://your-ip");
            options.WithCredentials("Administrator", "password");

            var cluster = await Cluster.ConnectAsync(options);
            // end::auth[]
        }
    }
}