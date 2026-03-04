# Authentication Migration Guide for Couchbase .NET SDK 3.9.0

Starting with version 3.9.0, the Couchbase .NET SDK introduces a cleaner, more explicit authentication model that matches the other Couchbase SDKs. And it's (almost) fully backwards compatible, but we recommend you migrate to the new API.

This guide walks you through each authentication method, how to migrate, and the edge cases you should know about.

---

## At a Glance

If you just want the tl;dr, here are the before/after snippets for each method:

### Password Authentication

**Before:**

```csharp
var options = new ClusterOptions { UserName = "admin", Password = "password" };
// or
var options = new ClusterOptions().WithCredentials("admin", "password");
```

**After:**

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithPasswordAuthentication("admin", "password");
```

### Certificate Authentication (mTLS)

**Before:**

```csharp
var clientCerts = new X509Certificate2Collection();
clientCerts.Add(new X509Certificate2("path/to/client-cert.pfx", "certPassword"));

var certFactory = new PredefinedCertificateFactory(clientCerts);
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithX509CertificateFactory(certFactory);
```

**After:**

```csharp
var clientCerts = new X509Certificate2Collection();
clientCerts.Add(new X509Certificate2("path/to/client-cert.pfx", "certPassword"));

var certFactory = new PredefinedCertificateFactory(clientCerts);
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithCertificateAuthentication(certFactory);
```

### JWT Authentication (new)

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithJwtAuthentication("your-jwt-token-here");
```

---

## Password Authentication

The most common way to connect. Uses SASL (PLAIN over TLS, SCRAM-SHA1 without TLS) for Key-Value operations, and HTTP Basic auth for Query, Search, Analytics, and management services.

**New way:**

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbase://localhost")
    .WithPasswordAuthentication("admin", "password");

var cluster = await Cluster.ConnectAsync(options);
```

Under the hood, `WithPasswordAuthentication` creates a `PasswordAuthenticator` and sets it on the options.

Authenticators are objects you can create yourself and pass to `WithAuthenticator()`:

```csharp
var authenticator = new PasswordAuthenticator("admin", "password");

var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithAuthenticator(authenticator);
```

> **Note:** The `Cluster.ConnectAsync("connStr", "user", "pass")` overload still works and is _not_ deprecated. It uses the legacy `UserName`/`Password` properties internally, but there are no plans to remove it.

---

## Certificate Authentication (mTLS)

For mutual TLS, the server verifies the client's identity via an X.509 certificate presented during the TLS handshake. There's no SASL exchange, authentication happens at the transport layer.

The SDK uses `CertificateAuthenticator` to hold the client certificate factory. This replaces the old pattern of setting `X509CertificateFactory` on `ClusterOptions`.

**New way:**

```csharp
var clientCerts = new X509Certificate2Collection();
clientCerts.Add(new X509Certificate2("path/to/client-cert.pfx", "certPassword"));

var certFactory = new PredefinedCertificateFactory(clientCerts);

var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithCertificateAuthentication(certFactory);
```

`WithCertificateAuthentication()` automatically sets `EnableTls = true`.

> **Important:** Only provide **client certificates** to `CertificateAuthenticator`. If you were previously bundling server CAs into `X509CertificateFactory`, those should now go into `TlsSettings.TrustedServerCertificateFactory` (covered in the [TLS Settings](#tls-settings) section below).

### Rotating Certificates

In environments where client certificates rotate periodically, you have two options.

**Option 1: `RotatingCertificateFactory`**

Wraps another certificate factory and periodically polls for new certificates:

```csharp
var baseCertFactory = new CertificateStoreFactory(searchCriteria);

var rotatingFactory = new RotatingCertificateFactory(
    certificateFactoryImplementation: baseCertFactory,
    interval: TimeSpan.FromHours(1),      // How often to check for new certs
    expiresIn: TimeSpan.FromDays(30),     // Only pick up certs valid for at least this long
    logger: loggerFactory.CreateLogger<RotatingCertificateFactory>()
);

var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithCertificateAuthentication(rotatingFactory);
```

The factory exposes a `HasUpdates` property. When it detects new certificates, the SDK's connection pool picks them up for new connections automatically, and the HTTP handler is re-configured.

**Option 2: Swap the `CertificateAuthenticator` at runtime**

If you need more control over _when_ certificate rotation happens, you can swap the entire authenticator:

```csharp
// Initial setup
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithCertificateAuthentication(initialCertFactory);

var cluster = await Cluster.ConnectAsync(options);

// Later, when certificates need to rotate
var newCertFactory = new PredefinedCertificateFactory(newClientCerts);
cluster.Authenticator(new CertificateAuthenticator(newCertFactory));
```

> **Note:** The new certificate applies to all **new** connections (both KV and HTTP). Existing KV connections are **not** closed or re-authenticated — they continue using the old certificate until they reconnect naturally. If you need all connections to use the new cert immediately, you'll need to dispose and re-bootstrap with `Cluster.ConnectAsync()`.

---

## JWT Authentication

JWT authentication is new in 3.9.0. It uses SASL OAUTHBEARER for Key-Value connections and Bearer token headers for HTTP services. Like certificate auth, it requires TLS.

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithJwtAuthentication("your-jwt-token-here");

var cluster = await Cluster.ConnectAsync(options);
```

`WithJwtAuthentication()` automatically sets `EnableTls = true`.

### Refreshing JWT Tokens

JWTs are usually short-lived. You need to swap in a fresh token before the current one expires, using `cluster.Authenticator()`:

```csharp
var freshToken = await GetNewTokenFromYourAuthProvider();
cluster.Authenticator(new JwtAuthenticator(freshToken));
```

When you swap in a new `JwtAuthenticator`, the SDK:

1. Uses the new token for all **new** connections immediately.
2. **Asynchronously re-authenticates** all existing Key-Value connections with the new token.

The `Authenticator()` call returns immediately, re-authentication happens in the background. If your token has already expired by the time re-authentication reaches a connection, you'll see `CouchbaseException` on operations against that connection.

> **Tip:** Don't wait until operations start failing to refresh your token.

---

## TLS Settings

TLS configuration has been reorganized. Previously, server certificate validation settings (trust anchors, name-mismatch flags, custom callbacks) were scattered across `ClusterOptions`. They now live in a dedicated `TlsSettings` object.

**Old way (deprecated):**

```csharp
var options = new ClusterOptions
{
    EnableTls = true,
    KvIgnoreRemoteCertificateNameMismatch = true,
    HttpIgnoreRemoteCertificateMismatch = true,
    EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13,
    KvCertificateCallbackValidation = (sender, certificate, chain, sslPolicyErrors) => {...},
    HttpCertificateCallbackValidation = (sender, certificate, chain, sslPolicyErrors) => {...}
};
```

**New way:**

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithPasswordAuthentication("admin", "password")
    .WithTlsSettings(tls =>
    {
        tls.KvIgnoreRemoteCertificateNameMismatch = true;
        tls.HttpIgnoreRemoteCertificateNameMismatch = true;
        tls.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
        tls.KvCertificateValidationCallback = MyKvCertificateValidator;
        tls.HttpCertificateValidationCallback = MyHttpCertificateValidator;
    });
```

> **Note:** The old properties on `ClusterOptions` (`KvIgnoreRemoteCertificateNameMismatch`, `HttpIgnoreRemoteCertificateMismatch`, `KvCertificateCallbackValidation`, `HttpCertificateCallbackValidation`, `EnabledSslProtocols`) are still functional — they delegate to `TlsSettings` internally. But they're marked `[Obsolete]` and will be removed.

### Trusting Custom Server CA Certificates

Previously, server CA certificates and client certificates were often mixed together in `X509CertificateFactory`. They're now properly separated:

- **Client certificates** → `WithCertificateAuthentication()` (for mTLS)
- **Server CA certificates** → `TlsSettings.TrustedServerCertificateFactory` (for validating the server)

To trust specific server CA certificates in `CustomRootTrust` (instead of relying on the system trust store):

```csharp
var serverCaCerts = new X509Certificate2Collection();
serverCaCerts.Add(new X509Certificate2("path/to/server-ca.pem"));

var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithPasswordAuthentication("admin", "password")
    .WithTrustedServerCertificates(serverCaCerts);
```

Or use a certificate factory:

```csharp
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithPasswordAuthentication("admin", "password")
    .WithTlsSettings(tls =>
    {
        tls.TrustedServerCertificateFactory = new PredefinedCertificateFactory(serverCaCerts);
    });
```

### TlsSettings Properties Reference

| Property | Description | Default |
|----------|-------------|---------|
| `TrustedServerCertificateFactory` | Factory providing server CA certificates for validation (custom root trust) | `null` (system trust store) |
| `KvIgnoreRemoteCertificateNameMismatch` | Ignore cert name mismatches for KV connections (dev only!) | `false` |
| `HttpIgnoreRemoteCertificateNameMismatch` | Ignore cert name mismatches for HTTP connections (dev only!) | `false` |
| `KvCertificateValidationCallback` | Custom callback for KV server cert validation | `null` |
| `HttpCertificateValidationCallback` | Custom callback for HTTP server cert validation | `null` |
| `EnabledSslProtocols` | Allowed SSL/TLS protocols | TLS 1.2 + TLS 1.3 (.NET 5+) |
| `EnableCertificateRevocation` | Check certificate revocation lists | `false` |

---

## Runtime Authenticator Swapping

The `cluster.Authenticator()` method lets you replace the authenticator on a live cluster. This is useful for:

1. **JWT token refresh** — Swap in a new token before the current one expires.
2. **Certificate rotation** — Provide new client certificates (alternative to `RotatingCertificateFactory`).
3. **Password rotation** — Update credentials without tearing down the cluster connection.

```csharp
var cluster = await Cluster.ConnectAsync(options);

// Swap authenticator at runtime
cluster.Authenticator(new PasswordAuthenticator("admin", "newPassword"));
```

### Constraints

- **You cannot change the authenticator type at runtime.** If you connected with `PasswordAuthenticator`, you must continue using `PasswordAuthenticator`. Attempting to switch to `CertificateAuthenticator` or `JwtAuthenticator` throws `InvalidArgumentException`. The SDK records the authenticator type at connection time and checks it on every swap.

- **For `JwtAuthenticator`:** Swapping triggers an asynchronous re-authentication of all existing KV connections using the new token. This is the only authenticator type that re-authenticates in-flight connections.

- **For `PasswordAuthenticator` and `CertificateAuthenticator`:** The new credentials apply to **new connections only**. Existing connections continue using the old credentials until they are naturally recycled.

---

## Legacy Precedence: What Happens When You Mix Old and New

If you set both the new `Authenticator` (via `WithPasswordAuthentication`, `WithCertificateAuthentication`, etc.) _and_ the legacy properties (`UserName`/`Password`, `X509CertificateFactory`), the SDK uses a precedence order when resolving the effective authenticator at connect time (in `GetEffectiveAuthenticator()`):

1. **Explicit `Authenticator`** — If an `IAuthenticator` was set (via any `With*Authentication` method or `WithAuthenticator`), it wins. The legacy properties are ignored entirely.
2. **`X509CertificateFactory`** — If no explicit authenticator was set but `X509CertificateFactory` is configured, the SDK creates a `CertificateAuthenticator` from it.
3. **`UserName` / `Password`** — If neither of the above is set, but `UserName` is non-empty, the SDK creates a `PasswordAuthenticator`.
4. **None** — If nothing is configured, the SDK throws `InvalidConfigurationException` at connect time.

In practice, this means:

```csharp
// ⚠️ If you do this, the PasswordAuthenticator wins and the cert factory is ignored:
var options = new ClusterOptions()
    .WithConnectionString("couchbases://localhost")
    .WithX509CertificateFactory(certFactory)         // sets X509CertificateFactory AND Authenticator
    .WithPasswordAuthentication("admin", "password"); // overwrites Authenticator

// The SDK connects with password auth, not certificate auth.
```

The simplest rule: **pick one `With*Authentication` method and don't mix it with the legacy properties.** If you're migrating, remove the old `UserName`/`Password`/`X509CertificateFactory` settings and replace them with the corresponding new method.

---

## Migration Checklist

- [ ] Replace `UserName`/`Password` properties with `WithPasswordAuthentication()`
- [ ] Replace `WithCredentials()` calls with `WithPasswordAuthentication()`
- [ ] Replace `WithX509CertificateFactory()` with `WithCertificateAuthentication()`
- [ ] Move `KvIgnoreRemoteCertificateNameMismatch`, `HttpIgnoreRemoteCertificateMismatch`, `EnabledSslProtocols`, and certificate validation callbacks into `WithTlsSettings()`
- [ ] Separate server CA certificates from client certificates — server CAs go in `TlsSettings.TrustedServerCertificateFactory` (or use `WithTrustedServerCertificates()`)
- [ ] For JWT auth: implement token refresh logic using `cluster.Authenticator()`
- [ ] Suppress any `[Obsolete]` warnings from old properties to verify your code compiles cleanly against 3.9.0

---

## Summary

| Old API | New API |
|---------|---------|
| `ClusterOptions.UserName` / `Password` | `WithPasswordAuthentication(username, password)` |
| `WithCredentials(username, password)` | `WithPasswordAuthentication(username, password)` |
| `X509CertificateFactory` (client certs) | `WithCertificateAuthentication(factory)` |
| `WithX509CertificateFactory(factory)` | `WithCertificateAuthentication(factory)` |
| `X509CertificateFactory` (server CAs) | `TlsSettings.TrustedServerCertificateFactory` or `WithTrustedServerCertificates()` |
| `KvIgnoreRemoteCertificateNameMismatch` | `TlsSettings.KvIgnoreRemoteCertificateNameMismatch` |
| `HttpIgnoreRemoteCertificateMismatch` | `TlsSettings.HttpIgnoreRemoteCertificateNameMismatch` |
| `KvCertificateCallbackValidation` | `TlsSettings.KvCertificateValidationCallback` |
| `HttpCertificateCallbackValidation` | `TlsSettings.HttpCertificateValidationCallback` |
| `EnabledSslProtocols` | `TlsSettings.EnabledSslProtocols` |
| N/A | `WithJwtAuthentication(token)` |
| N/A | `cluster.Authenticator(newAuthenticator)` |
