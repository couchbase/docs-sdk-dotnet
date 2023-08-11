using System;
using System.Buffers;
using System.IO;
using System.Threading.Tasks;
using System.Text.Json;
using System.Threading;
using Couchbase.Core.IO.Operations;
using Couchbase.Core.IO.Serializers;
using Couchbase.Core.IO.Transcoders;
using Couchbase.KeyValue;
using MessagePack;

namespace Couchbase.Examples
{
    public class Transcoding
    {
        private ICouchbaseCollection _collection;
        public Transcoding(ICouchbaseCollection collection)
        {
            _collection = collection;
        }

        public async Task RawJsonEncode()
        {
            // #tag::raw-json-encode[]
            var userBytes = JsonSerializer.SerializeToUtf8Bytes(new User
            {
                Name = "John Smith",
                Age = 27
            }, typeof(User));

            await _collection.UpsertAsync("john-smith", userBytes, options => options.Transcoder(new RawJsonTranscoder()));
            // #end::raw-json-encode[]
        }

        public async Task RawJsonDecode()
        {
            // #tag::raw-json-decode[]
            var result =
                await _collection.GetAsync("john-smith", options => options.Transcoder(new RawJsonTranscoder()));

            var returnedJson = result.ContentAs<byte[]>();
            var user = JsonSerializer.Deserialize(returnedJson, typeof(User));
            // #end::raw-json-decode[]
        }

        public async Task String()
        {
            // #tag::string[]
            var docId = "doc";

            await _collection.UpsertAsync<string>(docId, "hello world",
                options =>
                {
                    options.Transcoder(new RawStringTranscoder());
                    options.Timeout(TimeSpan.FromMinutes(1000));
                });


            var result = await _collection.GetAsync(docId, options => options.Transcoder(new RawStringTranscoder()));

            var returned = result.ContentAs<string>();
            // #end::string[]
            Console.WriteLine(returned);
        }

        public async Task Binary()
        {

            // #tag::binary[]
            var docId = "doc";

            var bytes = System.Text.Encoding.UTF8.GetBytes("hello world");

            await _collection.UpsertAsync(docId, bytes, options => options.Transcoder(new RawBinaryTranscoder()));

            var result = await _collection.GetAsync(docId, options => options.Transcoder(new RawBinaryTranscoder()));

            var returned = result.ContentAs<byte[]>();
            // #end::binary[]
            Console.WriteLine(returned);
        }

        public async Task BinaryMemory()
        {

            // #tag::binary-memory[]
            var docId = "doc";

            using var buffer = MemoryPool<byte>.Shared.Rent(16);
            var byteCount = System.Text.Encoding.UTF8.GetBytes("hello world", buffer.Memory.Span);
            Memory<byte> bytes = buffer.Memory.Slice(0, byteCount);

            await _collection.UpsertAsync(docId, bytes, options => options.Transcoder(new RawBinaryTranscoder()));

            var result = await _collection.GetAsync(docId, options => options.Transcoder(new RawBinaryTranscoder()));

            // Be sure to dispose of the IMemoryOwner<byte> when done, typically via a using statement
            using var returned = result.ContentAs<IMemoryOwner<byte>>();
            // #end::binary-memory[]
            Console.WriteLine(System.Text.Encoding.UTF8.GetString(returned.Memory.Span));
        }

        public async Task CustomEncode()
        {
            // #tag::custom-encode[]
            var serializer = new DotnetJsonSerializer();
            var transcoder = new JsonTranscoder(serializer);

            var user = new User
            {
                Name = "John Smith",
                Age = 27
            };

            await _collection.UpsertAsync("john-smith", user, options => options.Transcoder(transcoder));
            // #end::custom-encode[]
        }

        public async Task CustomDecode()
        {
            // #tag::custom-decode[]
            var serializer = new DotnetJsonSerializer();
            var transcoder = new JsonTranscoder(serializer);

            var user = new User
            {
                Name = "John Smith",
                Age = 27
            };

            var result = await _collection.GetAsync("john-smith", options => options.Transcoder(transcoder));
            var returnedUser = result.ContentAs<User>();
            // #end::custom-decode[]
        }

        public async Task Global()
        {
            // #tag::global[]
            var clusterOptions = new ClusterOptions().WithSerializer(new DotnetJsonSerializer());
            var cluster = await Cluster.ConnectAsync("couchbase://your-ip", clusterOptions);


            var results = await cluster.QueryAsync<dynamic>("SELECT * FROM `default`");
            await foreach (var result in results)
            {
                Console.WriteLine(result);
            }
            // #end::global[]
        }

        public async Task MsgPackEncode()
        {
            // #tag::msgpack-encode[]
            var serializer = new MsgPackSerializer();
            var transcoder = new MsgPackTranscoder(serializer);

            var user = new User2
            {
                Name = "John Smith",
                Age = 27
            };

            await _collection.UpsertAsync("john-smith", user, options => options.Transcoder(transcoder));
            // #end::msgpack-encode[]
        }

        public async Task MsgPackDecode()
        {
            // #tag::msgpack-decode[]
            var serializer = new MsgPackSerializer();
            var transcoder = new MsgPackTranscoder(serializer);

            var user = new User2
            {
                Name = "John Smith",
                Age = 27
            };

            var result = await _collection.GetAsync("john-smith", options => options.Transcoder(transcoder));
            var returnedUser = result.ContentAs<User2>();
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
}
