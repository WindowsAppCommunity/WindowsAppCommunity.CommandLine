using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Resolves relative path strings to IFile instances.
/// </summary>
public interface IAssetResolver
{
    /// <summary>
    /// Root folder for relative path resolution.
    /// </summary>
    IFolder SourceFolder { get; init; }

    /// <summary>
    /// Resolves a relative path string to an IFile instance.
    /// </summary>
    /// <param name="markdownSource">Markdown file for relative path context (varies per page).</param>
    /// <param name="relativePath">The relative path to resolve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved IFile, or null if not found.</returns>
    Task<IFile?> ResolveAsync(IFile markdownSource, string relativePath, CancellationToken ct = default);
}
