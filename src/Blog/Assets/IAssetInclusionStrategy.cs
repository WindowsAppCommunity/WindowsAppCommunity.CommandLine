using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Provides decision logic for asset inclusion via path rewriting.
/// Strategy returns rewritten path - path structure determines Include vs Reference behavior.
/// </summary>
public interface IAssetInclusionStrategy
{
    /// <summary>
    /// Decides asset inclusion strategy by returning rewritten path.
    /// </summary>
    /// <param name="referencingMarkdown">The markdown file that references the asset.</param>
    /// <param name="referencedAsset">The asset file being referenced.</param>
    /// <param name="originalPath">The original relative path from markdown.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>
    /// Rewritten path string. Path structure determines behavior:
    /// <list type="bullet">
    /// <item><description>Child path (no ../ prefix): Asset included in page folder (self-contained)</description></item>
    /// <item><description>Parent path (../ prefix): Asset referenced externally (link rewritten to account for folderization)</description></item>
    /// </list>
    /// </returns>
    Task<string> DecideAsync(
        IFile referencingMarkdown,
        IFile referencedAsset,
        string originalPath,
        CancellationToken ct = default);
}
