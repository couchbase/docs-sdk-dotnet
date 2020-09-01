using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.IO.Authentication.X509;

namespace sdk_docs_dotnet_examples
{
    public class Auth
    {
        public async Task Main(string[] args)
        {
            {
                // tag::basic[]
                try
                {
                    var cluster = await Cluster.ConnectAsync("couchbase://localhost", "username", "password");
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
                var options = new ClusterOptions().
                    WithX509CertificateFactory(CertificateFactory.GetCertificatesFromStore(
                        new CertificateStoreSearchCriteria
                        {
                            FindValue = "value",
                            X509FindType = X509FindType.FindBySubjectName,
                            StoreLocation = StoreLocation.CurrentUser,
                            StoreName = StoreName.CertificateAuthority
                        }));

                var cluster = await Cluster.ConnectAsync(options);
                // end::auth[]
            }
        }
    }
}
