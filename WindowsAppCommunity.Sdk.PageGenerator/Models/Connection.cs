namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record Connection(string Id, string Value)
{
    public static async Task<Connection> CreateAsync(IReadOnlyConnection sdkConnection, CancellationToken token = default)
    {
        var value = await sdkConnection.GetValueAsync(token);
        return new Connection(sdkConnection.Id, value);
    }
    
    public static async Task<List<Connection>> CreateAsync(IAsyncEnumerable<IReadOnlyConnection> sdkConnections, CancellationToken token = default)
    {
        List<Connection> connections = [];
        await foreach (var sdkConnection in sdkConnections.WithCancellation(token))
        {
            var connection = await CreateAsync(sdkConnection, token);
            connections.Add(connection);
        }
        return connections;
    }
}