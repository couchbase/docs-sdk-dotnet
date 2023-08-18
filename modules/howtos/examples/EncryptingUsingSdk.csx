// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script EncryptingUsingSdk.csx
//

#r "nuget: Couchbase.Extensions.Encryption, 2.0.0-dp.1"

using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Couchbase;
using Couchbase.Encryption;
using Couchbase.Encryption.Attributes;
using Couchbase.Encryption.Internal;
using Couchbase.KeyValue;
using Newtonsoft.Json.Linq;

await new EncryptingUsingSdk().ExampleAsync();

public class EncryptingUsingSdk
{
    public async Task ExampleAsync()
    {
        // tag::encrypting_using_sdk_1[]
        var provider =
            new AeadAes256CbcHmacSha512Provider(
                new AeadAes256CbcHmacSha512Cipher(), new Keyring(new IKey[]
                {
                    new Key("test-key", GetFakeKey(64))
                }));

        var cryptoManager = DefaultCryptoManager.Builder()
            .Decrypter(provider.Decrypter())
            .DefaultEncrypter(provider.Encrypter("test-key"))
            .Build();

        var encryptedTranscoder = new EncryptedFieldTranscoder(cryptoManager);

        var clusterOptions = new ClusterOptions()
            .WithTranscoder(encryptedTranscoder)
            .WithConnectionString("couchbase://your-ip")
            .WithCredentials("Administrator", "password");

        var cluster = await Cluster.ConnectAsync(clusterOptions);
        var bucket = await cluster.BucketAsync("travel-sample");
        // end::encrypting_using_sdk_1[]

        var id = Guid.NewGuid().ToString();

        var employee = new Employee
        {
            IsReplicant = true
        };

        // tag::encrypting_using_sdk_3[]
        var collection = await bucket.DefaultCollectionAsync();

        await collection.UpsertAsync(id, employee, options => { options.Expiry(TimeSpan.FromSeconds(10)); })
            .ConfigureAwait(false); //encrypts the IsReplicant field
        // end::encrypting_using_sdk_3[]

        await collection.UpsertAsync(id, employee, options =>
            {
                options.Transcoder(encryptedTranscoder);
                options.Expiry(TimeSpan.FromSeconds(10));
            })
            .ConfigureAwait(false);

        // tag::encrypting_using_sdk_4[]
        var getResult1 = await collection.GetAsync(id, options => options.Transcoder(encryptedTranscoder))
            .ConfigureAwait(false);

        var encrypted = getResult1.ContentAs<JObject>();
        Console.WriteLine(encrypted);
        // end::encrypting_using_sdk_4[]

        // tag::encrypting_using_sdk_5[]
        var getResult2 = await collection.GetAsync(id, options => options.Transcoder(encryptedTranscoder))
            .ConfigureAwait(false);

        var readItBack = getResult2.ContentAs<Employee>();
        Console.WriteLine(readItBack.IsReplicant);
        // end::encrypting_using_sdk_5[]
    }

    public void KeyStore()
    {
        // tag::encrypting_using_sdk_8[]
        var keyBytes = new Span<byte>(new byte[64]);
        RandomNumberGenerator.Fill(keyBytes);

        var keyRing = new Keyring(new IKey[]
        {
            new Key("my-key", keyBytes.ToArray())
        });
        // end::encrypting_using_sdk_8[]

        // tag::encrypting_using_sdk_9[]
        var provider =
            new AeadAes256CbcHmacSha512Provider(
                new AeadAes256CbcHmacSha512Cipher(), new Keyring(new IKey[]
                {
                    new Key("test-key", GetFakeKey(64))
                }));

        var cryptoManager = DefaultCryptoManager.Builder()
            .Decrypter(provider.Decrypter())
            .DefaultEncrypter(provider.Encrypter("test-key"))
            .Build();
        // end::encrypting_using_sdk_9[]
    }

    public void Migrating()
    {
        {
            // tag::legacy_field_name_prefix[]
            var cryptoManager = DefaultCryptoManager.Builder()
                .EncryptedFieldNamePrefix("__crypt_")
                //other config
                .Build();
            // end::legacy_field_name_prefix[]
        }

        {
            var keyring = new Keyring(new IKey[]
            {
                new Key("publickey", Encoding.UTF8.GetBytes("!mysecretkey#9^5usdk39d&dlf)03sL")),
                new Key("hmacKey", Encoding.UTF8.GetBytes("mysecret")),
                new Key("upgrade-key", GetFakeKey(64))
            });

            var provider = new AeadAes256CbcHmacSha512Provider(new AeadAes256CbcHmacSha512Cipher(), keyring);

            // tag::legacy_decrypters[]
            var cryptoManager = DefaultCryptoManager.Builder()
                .LegacyAesDecrypters(keyring, "hmacKey")
                .DefaultEncrypter(provider.Encrypter("upgrade-key"))
                .Decrypter(provider.Decrypter())
                .Build();
            // end::legacy_decrypters[]
        }
    }

    // tag::encrypting_using_sdk_2[]
    public class Employee
    {
        [EncryptedField(KeyName = "test-key")]
        public bool IsReplicant { get; set; }
    }
    // end::encrypting_using_sdk_2[]

    public static byte[] GetFakeKey(int len)
    {
        var result = new byte[len];
        for (var i = 0; i < len; i++)
        {
            result[i] = (byte)i;
        }
        return result;
    }
}