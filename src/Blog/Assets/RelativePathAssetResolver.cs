using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Resolves relative paths to IFile instances using source folder and markdown file context.
/// Paths are resolved relative to the markdown file's location (pre-folderization).
/// Stateless design - markdown source passed per-call to support shared resolver across pages.
/// </summary>
public sealed class RelativePathAssetResolver : IAssetResolver
{
    /// <inheritdoc/>
    public async Task<IFile?> ResolveAsync(IFile sourceFile, string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        try
        {
            // Normalize path separators to forward slash
            var normalizedPath = relativePath.Replace('\\', '/');

            // Resolve relative to markdown file's containing location (pre-folderization)
            // The markdown file itself is the base for relative path resolution
            var item = await sourceFile.GetItemByRelativePathAsync($"../{normalizedPath}", ct);

            // Return only if it's a file
            return item as IFile;
        }
        catch
        {
            // Path resolution failed (invalid path, not found, etc.)
            return null;
        }
    }
}
