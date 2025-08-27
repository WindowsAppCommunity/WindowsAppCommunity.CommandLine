using Markdig;

namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record Entity
{
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string Id { get; init; }
    
    /// <summary>
    /// The name of the entity.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// A description of the entity, rendered as HTML.
    /// </summary>
    public string DescriptionHtml { get; init; }

    /// <summary>
    /// An extended description of the entity, rendered as HTML.
    /// </summary>
    public string ExtendedDescriptionHtml { get; init; }

    /// <summary>
    /// A description of the entity, rendered as plaintext.
    /// </summary>
    public string DescriptionPlain { get; init; }

    /// <summary>
    /// An extended description of the entity, rendered as plaintext.
    /// </summary>
    public string ExtendedDescriptionPlain { get; init; }

    /// <summary>
    /// The extended description of the entity if it exists, otherwise the regular description, rendered as HTML.
    /// </summary>
    public string LongestDescriptionHtml { get; init; }

    /// <summary>
    /// The extended description of the entity if it exists, otherwise the regular description, rendered as plaintext.
    /// </summary>
    public string LongestDescriptionPlain { get; init; }
    
    /// <inheritdoc cref="IReadOnlyEntity.Links"/>
    public List<Link> Links { get; init; }
    
    /// <summary>
    /// The connections associated with this entity.
    /// </summary>
    public List<Connection> Connections { get; init; }
    
    /// <summary>
    /// The image files associated with this entity.
    /// </summary>
    public List<Image> Images { get; init; }

    public static async Task<Entity> CreateAsync(IReadOnlyEntity sdkEntity, CancellationToken token = default)
    {
        var descriptionHtml = Markdown.ToHtml(sdkEntity.Description);
        var extendedDescriptionHtml = Markdown.ToHtml(sdkEntity.ExtendedDescription);
        var descriptionPlain = Markdown.ToPlainText(sdkEntity.Description);
        var extendedDescriptionPlain = Markdown.ToPlainText(sdkEntity.ExtendedDescription);
        var hasExtendedDescription = string.IsNullOrWhiteSpace(extendedDescriptionPlain);

        return new Entity
        {
            Id = sdkEntity.Id,
            Name = sdkEntity.Name,
            DescriptionHtml = descriptionHtml,
            ExtendedDescriptionHtml = extendedDescriptionHtml,
            DescriptionPlain = descriptionPlain,
            ExtendedDescriptionPlain = extendedDescriptionPlain,
            LongestDescriptionHtml = hasExtendedDescription
                ? descriptionHtml : extendedDescriptionHtml,
            LongestDescriptionPlain = hasExtendedDescription
                ? descriptionPlain : extendedDescriptionPlain,
            Links = sdkEntity.Links.ToList(),
            Connections = await Connection.CreateAsync(sdkEntity.GetConnectionsAsync(token), token),
            Images = await Image.CreateAsync(sdkEntity.GetImageFilesAsync(token), token)
        };
    }
}