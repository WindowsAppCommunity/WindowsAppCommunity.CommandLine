using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Detects relative asset links in rendered HTML output.
/// </summary>
public interface IAssetLinkDetector
{
    /// <summary>
    /// Detects relative asset link strings in rendered HTML output.
    /// </summary>
    /// <param name="sourceFile">File instance containing text to detect links from.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Async enumerable of relative path strings.</returns>
    IAsyncEnumerable<string> DetectAsync(IFile sourceFile, CancellationToken cancellationToken = default);
}