using Newtonsoft.Json;
using OwlCore.ComponentModel;
using OwlCore.Extensions;
using System.Text;

namespace WindowsAppCommunity.CommandLine;

/// <summary>
/// An <see cref="IAsyncSerializer{TSerialized}"/> and implementation for serializing and deserializing streams using System.Text.Json.
/// </summary>
public class NewtonsoftSerializer : IAsyncSerializer<Stream>, ISerializer<Stream>
{
    /// <summary>
    /// A singleton instance for <see cref="NewtonsoftSerializer"/>.
    /// </summary>
    public static NewtonsoftSerializer Singleton { get; } = new();

    /// <inheritdoc />
    public Task<Stream> SerializeAsync<T>(T data, CancellationToken? cancellationToken = null) => Task.Run(() => Serialize(data), cancellationToken ?? CancellationToken.None);

    /// <inheritdoc />
    public Task<Stream> SerializeAsync(Type inputType, object data, CancellationToken? cancellationToken = null) => Task.Run(() => Serialize(inputType, data), cancellationToken ?? CancellationToken.None);

    /// <inheritdoc />
    public Task<TResult> DeserializeAsync<TResult>(Stream serialized, CancellationToken? cancellationToken = null) => Task.Run(() => Deserialize<TResult>(serialized), cancellationToken ?? CancellationToken.None);

    /// <inheritdoc />
    public Task<object> DeserializeAsync(Type returnType, Stream serialized, CancellationToken? cancellationToken = null) => Task.Run(() => Deserialize(returnType, serialized), cancellationToken ?? CancellationToken.None);

    /// <inheritdoc />
    public Stream Serialize<T>(T data)
    {
        var res = JsonConvert.SerializeObject(data, typeof(T), null);
        return new MemoryStream(Encoding.UTF8.GetBytes(res));
    }

    /// <inheritdoc />
    public Stream Serialize(Type type, object data)
    {
        var res = JsonConvert.SerializeObject(data, type, null);
        return new MemoryStream(Encoding.UTF8.GetBytes(res));
    }

    /// <inheritdoc />
    public TResult Deserialize<TResult>(Stream serialized)
    {
        serialized.Position = 0;
        var str = Encoding.UTF8.GetString(serialized.ToBytes());
        return (TResult)JsonConvert.DeserializeObject(str, typeof(TResult))!;
    }

    /// <inheritdoc />
    public object Deserialize(Type type, Stream serialized)
    {
        serialized.Position = 0;
        var str = Encoding.UTF8.GetString(serialized.ToBytes());
        return JsonConvert.DeserializeObject(str, type)!;
    }
}
