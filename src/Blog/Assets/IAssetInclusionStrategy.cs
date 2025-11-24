using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Provides decision logic for asset inclusion via path rewriting.
/// Strategy returns rewritten path - path structure determines Include vs Reference behavior.
/// </summary>
public interface IAssetStrategy
{
    /// <summary>
    /// Decides asset inclusion strategy by returning rewritten path.
    /// </summary>
    /// <param name="referencingTextFile">The file that references the asset.</param>
    /// <param name="referencedAssetFile">The asset file being referenced.</param>
    /// <param name="originalPath">The original relative path from the file.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Rewritten path string. Path structure determines behavior:
    /// <list type="bullet">
    /// <item><description>Child path (no ../ prefix): Asset included in page folder (self-contained)</description></item>
    /// <item><description>Parent path (../ prefix): Asset referenced externally (link rewritten to account for folderization)</description></item>
    /// <item><description>null: Asset has been dropped from output without inclusion or reference.</description></item>
    /// </list>
    /// </returns>
    Task<string?> DecideAsync(IFile referencingTextFile, IFile referencedAssetFile, string originalPath, CancellationToken ct = default);
}
