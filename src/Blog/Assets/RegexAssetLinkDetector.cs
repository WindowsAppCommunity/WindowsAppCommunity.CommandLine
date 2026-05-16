using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Detects relative asset links in markdown and HTML text.
/// </summary>
public sealed partial class RegexAssetLinkDetector : IAssetLinkDetector
{
    /// <summary>
    /// Regex pattern for markdown links and images.
    /// </summary>
    [GeneratedRegex("""!?\[[^\]]*\]\((?<path>[^)\s]+)(?:\s+[^)]*)?\)""", RegexOptions.Compiled)]
    private static partial Regex MarkdownLinkPattern();

    /// <summary>
    /// Regex pattern for HTML href/src attributes.
    /// </summary>
    [GeneratedRegex("""(?:href|src)\s*=\s*["'](?<path>[^"']+)["']""", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex HtmlAttributePattern();

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> DetectAsync(IFile source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var text = await source.ReadTextAsync(ct);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (Match match in MarkdownLinkPattern().Matches(text))
        {
            if (ct.IsCancellationRequested)
                yield break;

            var path = match.Groups["path"].Value;
            if (!ShouldYield(path, seen))
                continue;

            yield return path;
        }

        foreach (Match match in HtmlAttributePattern().Matches(text))
        {
            if (ct.IsCancellationRequested)
                yield break;

            var path = match.Groups["path"].Value;
            if (!ShouldYield(path, seen))
                continue;

            yield return path;
        }
    }

    private static bool ShouldYield(string path, HashSet<string> seen)
    {
        if (string.IsNullOrWhiteSpace(path))
            return false;

        path = path.Trim().Trim('<', '>');

        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (path.StartsWith('#') || path.StartsWith('/') || path.StartsWith('\\'))
            return false;
        if (path.StartsWith("//", StringComparison.Ordinal))
            return false;
        if (path.Contains("://", StringComparison.Ordinal))
            return false;
        if (path.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("javascript:", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("tel:", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return seen.Add(path);
    }
}