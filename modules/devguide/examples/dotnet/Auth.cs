using System;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Security.Authentication;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Core.IO.Authentication;
using Couchbase.Core.IO.Authentication.X509;
using Couchbase.Core.IO.Authentication.Authenticators;

namespace Couchbase.Docs.Examples.Howtos.Auth;

public class Auth
{
    public async Task ExecuteAsync()
    {
        {
            // tag::password[]
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "password");

            var cluster = await Cluster.ConnectAsync(options);
            // end::password[]
        }

        {
            // tag::password-authenticator[]
            var authenticator = new PasswordAuthenticator("Administrator", "password");

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithAuthenticator(authenticator);

            var cluster = await Cluster.ConnectAsync(options);
            // end::password-authenticator[]
        }

        {
            // tag::password-before[]
            // Old: setting credentials via properties or WithCredentials()
            var options = new ClusterOptions
            {
                UserName = "Administrator",
                Password = "password"
            };
            // or: new ClusterOptions().WithCredentials("Administrator", "password");
            options.WithConnectionString("couchbases://your-ip");

            var cluster = await Cluster.ConnectAsync(options);
            // end::password-before[]
        }

        {
            // tag::password-after[]
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "password");

            var cluster = await Cluster.ConnectAsync(options);
            // end::password-after[]
        }

        {
            // tag::cert[]
            var clientCerts = new X509Certificate2Collection();
            clientCerts.Add(new X509Certificate2("path/to/client-cert.pfx", "certPassword"));

            var certFactory = new PredefinedCertificateFactory(clientCerts);

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithCertificateAuthentication(certFactory);

            var cluster = await Cluster.ConnectAsync(options);
            // end::cert[]
        }

        {
            // tag::cert-before[]
            // Old: using WithX509CertificateFactory
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithX509CertificateFactory(CertificateFactory.GetCertificatesFromStore(
                    new CertificateStoreSearchCriteria
                    {
                        FindValue = "value",
                        X509FindType = X509FindType.FindBySubjectName,
                        StoreLocation = StoreLocation.CurrentUser,
                        StoreName = StoreName.CertificateAuthority
                    }));

            var cluster = await Cluster.ConnectAsync(options);
            // end::cert-before[]
        }

        {
            // tag::cert-after[]
            var certFactory = CertificateFactory.GetCertificatesFromStore(
                new CertificateStoreSearchCriteria
                {
                    FindValue = "value",
                    X509FindType = X509FindType.FindBySubjectName,
                    StoreLocation = StoreLocation.CurrentUser,
                    StoreName = StoreName.CertificateAuthority
                });

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithCertificateAuthentication(certFactory);

            var cluster = await Cluster.ConnectAsync(options);
            // end::cert-after[]
        }

        {
            // tag::cert-rotating[]
            var baseCertFactory = CertificateFactory.GetCertificatesFromStore(
                new CertificateStoreSearchCriteria
                {
                    FindValue = "value",
                    X509FindType = X509FindType.FindBySubjectName,
                    StoreLocation = StoreLocation.CurrentUser,
                    StoreName = StoreName.CertificateAuthority
                });

            var rotatingFactory = new RotatingCertificateFactory(
                certificateFactoryImplementation: baseCertFactory,
                interval: TimeSpan.FromHours(1),   // Check for new certs every hour
                expiresIn: TimeSpan.FromDays(30),  // Only pick up certs valid for at least 30 days
                logger: null                       // Pass a real ILogger<RotatingCertificateFactory> in production
            );

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithCertificateAuthentication(rotatingFactory);

            var cluster = await Cluster.ConnectAsync(options);
            // end::cert-rotating[]
        }

        {
            // tag::cert-rotate-manual[]
            var initialCertFactory = new PredefinedCertificateFactory(new X509Certificate2Collection());

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithCertificateAuthentication(initialCertFactory);

            var cluster = await Cluster.ConnectAsync(options);

            // Later, when certificates need to rotate:
            var newClientCerts = new X509Certificate2Collection();
            newClientCerts.Add(new X509Certificate2("path/to/new-client-cert.pfx", "newCertPassword"));

            var newCertFactory = new PredefinedCertificateFactory(newClientCerts);
            ((IClusterAuthenticator)cluster).Authenticator(new CertificateAuthenticator(newCertFactory));
            // end::cert-rotate-manual[]
        }

        {
            // tag::swap-authenticator[]
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "oldPassword");

            var cluster = await Cluster.ConnectAsync(options);

            // When credentials change, swap in a new authenticator.
            // New connections will use the updated credentials immediately;
            // existing connections continue with the old credentials until recycled.
            ((IClusterAuthenticator)cluster).Authenticator(new PasswordAuthenticator("Administrator", "newPassword"));
            // end::swap-authenticator[]
        }

        {
            // tag::tls-before[]
            // Old: TLS options set directly on ClusterOptions (deprecated in 3.9.0)
            var options = new ClusterOptions
            {
                EnableTls = true,
                KvIgnoreRemoteCertificateNameMismatch = true,
                HttpIgnoreRemoteCertificateMismatch = true,
                EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
            };
            options.WithConnectionString("couchbases://your-ip");
            options.WithCredentials("Administrator", "password");
            // end::tls-before[]
        }

        {
            // tag::tls-after[]
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "password")
                .WithTlsSettings(tls =>
                {
                    // Dev environments only — do not use in production.
                    tls.KvIgnoreRemoteCertificateNameMismatch = true;
                    tls.HttpIgnoreRemoteCertificateNameMismatch = true;
                    tls.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                });
            // end::tls-after[]
        }

        {
            // tag::trust-server-ca[]
            var serverCaCerts = new X509Certificate2Collection();
            serverCaCerts.Add(new X509Certificate2("path/to/server-ca.pem"));

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "password")
                .WithTrustedServerCertificates(serverCaCerts);
            // end::trust-server-ca[]
        }

        {
            // tag::trust-server-ca-factory[]
            var serverCaCerts = new X509Certificate2Collection();
            serverCaCerts.Add(new X509Certificate2("path/to/server-ca.pem"));

            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithPasswordAuthentication("Administrator", "password")
                .WithTlsSettings(tls =>
                {
                    tls.TrustedServerCertificateFactory = new PredefinedCertificateFactory(serverCaCerts);
                });
            // end::trust-server-ca-factory[]
        }

        {
            // tag::precedence-warning[]
            // Don't do this. WithPasswordAuthentication sets an explicit Authenticator
            // which takes precedence over the cert factory below — the SDK connects with
            // password auth and the certificate factory is silently ignored.
            var certFactory = new PredefinedCertificateFactory(new X509Certificate2Collection());
            var options = new ClusterOptions()
                .WithConnectionString("couchbases://your-ip")
                .WithX509CertificateFactory(certFactory)         // sets X509CertificateFactory AND Authenticator
                .WithPasswordAuthentication("Administrator", "password"); // overwrites Authenticator
            // end::precedence-warning[]
        }

        {
            // tag::basic[]
            try
            {
                var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
                // use the cluster
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect: {e}");
            }
            // end::basic[]
        }
    }
}