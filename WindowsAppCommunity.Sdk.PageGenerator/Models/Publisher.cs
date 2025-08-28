namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record Publisher
{
    // TODO: Complete publisher model
    public Entity Entity { get; init; }

    public static async Task<Publisher> CreateAsync(IReadOnlyPublisher publisher, CancellationToken token = default)
    {
        return new Publisher
        {
            Entity = await Entity.CreateAsync(publisher, token),
        };
    }
}