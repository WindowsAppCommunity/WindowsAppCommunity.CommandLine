using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OwlCore.Diagnostics;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Detects relative asset links in rendered using path-pattern regex (no element parsing).
/// </summary>
public sealed partial class RegexAssetLinkDetector : IAssetLinkDetector
{
    /// <summary>
    /// Regex pattern for relative path segments: alphanumerics, underscore, hyphen, dot.
    /// Matches paths with optional ./ or ../ prefixes and / or \ separators.
    /// </summary>
    [GeneratedRegex(@"(?:\.\.?/(?:[A-Za-z0-9_\-\.]+/)*[A-Za-z0-9_\-\.]+|[A-Za-z0-9_\-\.]+(?:/[A-Za-z0-9_\-\.]+)+)", RegexOptions.Compiled)]
    private static partial Regex RelativePathPattern();

    /// <summary>
    /// Regex pattern to detect protocol schemes (e.g., http://, custom://, drive://).
    /// </summary>
    [GeneratedRegex(@"[A-Za-z][A-Za-z0-9+\-\.]*://", RegexOptions.Compiled)]
    private static partial Regex ProtocolSchemePattern();

    [GeneratedRegex(@"\b[A-Za-z0-9_\-]+\.[A-Za-z0-9]+\b", RegexOptions.Compiled)]
    private static partial Regex FilenamePattern();

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> DetectAsync(IFile source, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var text = await source.ReadTextAsync(ct);

        foreach (Match match in RelativePathPattern().Matches(text))
        {
            if (ct.IsCancellationRequested)
                yield break;

            var path = match.Value;

            // Filter out non-relative patterns
            if (string.IsNullOrWhiteSpace(path))
                continue;

            // Exclude absolute root paths (optional - treating these as non-relative)
            if (path.StartsWith('/') || path.StartsWith('\\'))
                continue;

            // Check if this path is preceded by a protocol scheme (e.g., custom://path/to/file)
            // Look back to see if there's a protocol before this match
            var startIndex = match.Index;
            if (startIndex > 0)
            {
                // Check up to 50 characters before the match for a protocol scheme
                var lookbackLength = Math.Min(50, startIndex);
                var precedingText = text.Substring(startIndex - lookbackLength, lookbackLength);

                // If the preceding text ends with a protocol scheme (e.g., "custom://"), skip this match
                if (ProtocolSchemePattern().IsMatch(precedingText) && precedingText.TrimEnd().EndsWith("://"))
                    continue;
            }

            yield return path;
        }

        foreach (Match match in FilenamePattern().Matches(text))
        {
            yield return match.Value;
        }
    }
}
