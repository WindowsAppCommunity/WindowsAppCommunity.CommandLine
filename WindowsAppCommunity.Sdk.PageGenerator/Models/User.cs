namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

/// <summary>
/// A model representing a User entity with all async information fetched.
/// </summary>
public record User
{
    /// <summary>
    /// Common information about this entity.
    /// </summary>
    public Entity Entity { get; init; }
    
    public static async Task<User> CreateAsync(IReadOnlyUser sdkUser, CancellationToken token = default)
    {
        return new User
        {
            Entity = await Entity.CreateAsync(sdkUser, token)
        };
    }
}