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
    /// <param name="htmlSource">Virtual IFile representing rendered HTML output (in-memory representation).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Async enumerable of relative path strings.</returns>
    IAsyncEnumerable<string> DetectAsync(IFile htmlSource, CancellationToken ct = default);
}