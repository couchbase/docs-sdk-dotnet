// Run this using dotnet-script: https://github.com/filipw/dotnet-script
//
//      dotnet script Transcoding.csx
//

#r "nuget: CouchbaseNetClient, 3.4.8"
#r "nuget: MessagePack, 2.4.35"

using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using Couchbase;
using Couchbase.Core.IO.Operations;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.IO.Transcoders;
using Couchbase.KeyValue;
using MessagePack;

await new Transcoding().ExecuteAsync();
public class Transcoding
{
    public async Task ExecuteAsync()
    {
        var cluster = await Cluster.ConnectAsync("couchbase://your-ip", "Administrator", "password");
        var bucket = await cluster.BucketAsync("travel-sample");
        var scope = await bucket.ScopeAsync("inventory");
        var collection = await scope.CollectionAsync("default");
    
        // #tag::raw-json-encode[]
        var userBytes = JsonSerializer.SerializeToUtf8Bytes(new User
        {
            Name = "John Smith",
            Age = 27
        }, typeof(User));

        await collection.UpsertAsync("john-smith", userBytes, options => options.Transcoder(new RawJsonTranscoder()));
        // #end::raw-json-encode[]

        // #tag::raw-json-decode[]
        var rawJsonDecodeResult =
            await collection.GetAsync("john-smith", options => options.Transcoder(new RawJsonTranscoder()));

        var returnedJson = rawJsonDecodeResult.ContentAs<byte[]>();
        var decodedUser = JsonSerializer.Deserialize(returnedJson, typeof(User));
        // #end::raw-json-decode[]

        // #tag::string[]
        var docId = "doc";

        await collection.UpsertAsync<string>(docId, "hello world",
            options =>
            {
                options.Transcoder(new RawStringTranscoder());
                options.Timeout(TimeSpan.FromMinutes(1000));
            });


        var stringResult = await collection.GetAsync(docId, options => options.Transcoder(new RawStringTranscoder()));

        var returnedString = stringResult.ContentAs<string>();
        // #end::string[]
        Console.WriteLine(returnedString);

        // #tag::binary[]

        var strBytes = System.Text.Encoding.UTF8.GetBytes("hello world");

        await collection.UpsertAsync(docId, strBytes, options => options.Transcoder(new RawBinaryTranscoder()));

        var binaryResult = await collection.GetAsync(docId, options => options.Transcoder(new RawBinaryTranscoder()));

        var returnedBinary = binaryResult.ContentAs<byte[]>();
        // #end::binary[]
        Console.WriteLine(returnedBinary);

        // #tag::binary-memory[]

        using var buffer = MemoryPool<byte>.Shared.Rent(16);
        var byteCount = System.Text.Encoding.UTF8.GetBytes("hello world", buffer.Memory.Span);
        Memory<byte> bytes = buffer.Memory.Slice(0, byteCount);

        await collection.UpsertAsync(docId, bytes, options => options.Transcoder(new RawBinaryTranscoder()));

        var binaryMemoryResult = await collection.GetAsync(docId, options => options.Transcoder(new RawBinaryTranscoder()));

        // Be sure to dispose of the IMemoryOwner<byte> when done, typically via a using statement
        using var binary = binaryMemoryResult.ContentAs<IMemoryOwner<byte>>();
        // #end::binary-memory[]
        Console.WriteLine(System.Text.Encoding.UTF8.GetString(binary.Memory.Span));

        // #tag::custom-encode[]
        var serializer = new DotnetJsonSerializer();
        var transcoder = new JsonTranscoder(serializer);

        var user = new User
        {
            Name = "John Smith",
            Age = 27
        };

        await collection.UpsertAsync("john-smith", user, options => options.Transcoder(transcoder));
        // #end::custom-encode[]

        // #tag::custom-decode[]

        var customDecodeResult = await collection.GetAsync("john-smith", options => options.Transcoder(transcoder));
        var returnedUser = customDecodeResult.ContentAs<User>();
        // #end::custom-decode[]

        // #tag::global[]
        var newClusterOptions = new ClusterOptions().WithSerializer(new DotnetJsonSerializer());
        var newCluster = await Cluster.ConnectAsync("couchbase://your-ip", newClusterOptions);


        var globalResults = await newCluster.QueryAsync<dynamic>("SELECT * FROM `default`");
        await foreach (var result in globalResults)
        {
            Console.WriteLine(result);
        }
        // #end::global[]

        // #tag::msgpack-encode[]
        var msgpackSerializer = new MsgPackSerializer();
        var msgpackTranscoder = new MsgPackTranscoder(msgpackSerializer);

        var user2 = new User2
        {
            Name = "John Smith",
            Age = 27
        };

        await collection.UpsertAsync("john-smith", user2, options => options.Transcoder(msgpackTranscoder));
        // #end::msgpack-encode[]

        // #tag::msgpack-decode[]

        var msgpackResult = await collection.GetAsync("john-smith", options => options.Transcoder(msgpackTranscoder));
        var msgpackReturnedUser = msgpackResult.ContentAs<User2>();
        // #end::msgpack-decode[]
    }
}

public class User
{
    public string Name { get; set; }

    public int Age { get; set; }
}

//#tag::msg-pack-poco
[MessagePackObject]
public class User2
{
    [Key(0)]
    public string Name { get; set; }

    [Key(1)]
    public int Age { get; set; }
}
//#end::msg-pack-poco

//#tag::msgpack-serializer[]
public class MsgPackSerializer : ITypeSerializer
{
    public T Deserialize<T>(ReadOnlyMemory<byte> buffer)
    {
        return MessagePackSerializer.Deserialize<T>(buffer);
    }

    public T Deserialize<T>(Stream stream)
    {
        return MessagePackSerializer.Deserialize<T>(stream);
    }

    public ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        return MessagePackSerializer.DeserializeAsync<T>(stream, null, cancellationToken);
    }

    public void Serialize(Stream stream, object obj)
    {
        MessagePackSerializer.Serialize(stream, obj);
    }

    public ValueTask SerializeAsync(Stream stream, object obj, CancellationToken cancellationToken = default)
    {
        return new ValueTask(MessagePackSerializer.SerializeAsync(stream, obj, null, cancellationToken));
    }
}
//#end::msgpack-serializer[]

//#tag::msgpack-transcoder[]
public class MsgPackTranscoder : BaseTranscoder
{
    public MsgPackTranscoder() : this(new MsgPackSerializer())
    {
    }

    internal MsgPackTranscoder(MsgPackSerializer serializer)
    {
        Serializer = serializer;
    }

    public override Flags GetFormat<T>(T value)
    {
        var typeCode = Type.GetTypeCode(typeof(T));
        var dataFormat = DataFormat.Binary;
        return new Flags { Compression = Compression.None, DataFormat = dataFormat, TypeCode = typeCode };
    }

    public override void Encode<T>(Stream stream, T value, Flags flags, OpCode opcode)
    {
        Serializer.Serialize(stream, value);
    }

    public override T Decode<T>(ReadOnlyMemory<byte> buffer, Flags flags, OpCode opcode)
    {
        return Serializer.Deserialize<T>(buffer);
    }
}
//#end::msgpack-transcoder[]

//#tag::dotnet-serializer[]
public class DotnetJsonSerializer : ITypeSerializer
{
    public T Deserialize<T>(ReadOnlyMemory<byte> buffer)
    {
        return JsonSerializer.Deserialize<T>(buffer.Span);
    }

    public T Deserialize<T>(Stream stream)
    {
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        var span = new ReadOnlySpan<byte>(ms.GetBuffer()).Slice(0, (int)ms.Length);
        return JsonSerializer.Deserialize<T>(span);
    }

    public ValueTask<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        return JsonSerializer.DeserializeAsync<T>(stream, null, cancellationToken);
    }

    public void Serialize(Stream stream, object obj)
    {
        using var jsonUtf8Writer = new Utf8JsonWriter(stream);
        JsonSerializer.Serialize(jsonUtf8Writer, obj);
    }

    public ValueTask SerializeAsync(Stream stream, object obj, CancellationToken cancellationToken = default)
    {
        return new ValueTask(JsonSerializer.SerializeAsync(stream, obj, null, cancellationToken));
    }
}
//#end::dotnet-serializer[]
