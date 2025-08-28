namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record PublisherRole
{
    public Publisher Publisher { get; init; }
    
    public Role Role { get; init; }

    public static async Task<PublisherRole> CreateAsync(IReadOnlyPublisherRole sdkPublisherRole, CancellationToken token = default)
    {
        return new PublisherRole
        {
            Publisher = await Publisher.CreateAsync(sdkPublisherRole, token),
            Role = sdkPublisherRole.Role
        };
    }
    
    public static async Task<List<PublisherRole>> CreateAsync(IAsyncEnumerable<IReadOnlyPublisherRole> sdkPublisherRoles, CancellationToken token = default)
    {
        List<PublisherRole> publisherRoles = [];
        await foreach (var sdkPublisherRole in sdkPublisherRoles.WithCancellation(token))
        {
            var publisherRole = await CreateAsync(sdkPublisherRole, token);
            publisherRoles.Add(publisherRole);
        }
        return publisherRoles;
    }
}