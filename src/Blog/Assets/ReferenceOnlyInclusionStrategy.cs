using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Reference-only inclusion strategy: always rewrites paths to add one level of parent navigation,
/// treating all assets as externally referenced (not included in page folder).
/// </summary>
public sealed class ReferenceOnlyInclusionStrategy : IAssetInclusionStrategy
{
    /// <inheritdoc/>
    public Task<string> DecideAsync(
        IFile referencingMarkdown,
        IFile referencedAsset,
        string originalPath,
        CancellationToken ct = default)
    {
        // Reference-only behavior: add one parent directory prefix to account for folderization
        // Markdown file becomes folder/index.html, so links need one extra "../" to reach original location
        if (string.IsNullOrWhiteSpace(originalPath))
            return Task.FromResult(originalPath);

        var rewrittenPath = "../" + originalPath;
        return Task.FromResult(rewrittenPath);
    }
}
