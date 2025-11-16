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
    /// Markdown file for relative path context.
    /// </summary>
    IFile MarkdownSource { get; init; }

    /// <summary>
    /// Resolves a relative path string to an IFile instance.
    /// </summary>
    /// <param name="relativePath">The relative path to resolve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved IFile, or null if not found.</returns>
    Task<IFile?> ResolveAsync(string relativePath, CancellationToken ct = default);
}
