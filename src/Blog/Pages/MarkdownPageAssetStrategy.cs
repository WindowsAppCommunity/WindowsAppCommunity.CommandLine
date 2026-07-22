using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;

namespace WindowsAppCommunity.Blog.Pages;

/// <summary>
/// Rewrites markdown-to-markdown links to generated page routes, delegating ordinary asset behavior.
/// </summary>
public sealed class MarkdownPageAssetStrategy : IAssetStrategy
{
    /// <summary>
    /// Gets the source-derived route index for generated markdown pages.
    /// </summary>
    public required MarkdownPageRouteIndex RouteIndex { get; init; }

    /// <summary>
    /// Gets the strategy used for non-markdown assets.
    /// </summary>
    public required IAssetStrategy AssetStrategy { get; init; }

    /// <inheritdoc/>
    public Task<string?> DecideAsync(IFile referencingTextFile, IFile referencedAssetFile, string originalPath, CancellationToken ct = default)
    {
        if (!Path.GetExtension(referencedAssetFile.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
            return AssetStrategy.DecideAsync(referencingTextFile, referencedAssetFile, originalPath, ct);

        if (RouteIndex.TryGetRelativeRoute(referencingTextFile, referencedAssetFile, out var relativeRoute))
            return Task.FromResult<string?>($"{relativeRoute}{GetFragment(originalPath)}");

        Logger.LogWarning($"Markdown link target was resolved but is not part of the generated page route index: {referencedAssetFile.Name}");
        return Task.FromResult<string?>(null);
    }

    private static string GetFragment(string path)
    {
        var fragmentIndex = path.IndexOf('#');
        return fragmentIndex < 0 ? string.Empty : path[fragmentIndex..];
    }
}