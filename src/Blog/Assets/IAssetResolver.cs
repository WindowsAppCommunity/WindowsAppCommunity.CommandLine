using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Resolves relative path strings to IFile instances.
/// </summary>
public interface IAssetResolver
{
    /// <summary>
    /// Resolves a relative path string to an IFile instance.
    /// </summary>
    /// <param name="sourceFile">The file to get the relative path from.</param>
    /// <param name="relativePath">The relative path to resolve.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The resolved IFile, or null if not found.</returns>
    Task<IFile?> ResolveAsync(IFile sourceFile, string relativePath, CancellationToken ct = default);
}
