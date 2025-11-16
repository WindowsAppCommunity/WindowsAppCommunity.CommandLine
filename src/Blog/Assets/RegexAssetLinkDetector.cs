using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Detects relative asset links in rendered HTML using path-pattern regex (no element parsing).
/// </summary>
public sealed partial class RegexAssetLinkDetector : IAssetLinkDetector
{
    /// <summary>
    /// Regex pattern for relative path segments: alphanumerics, underscore, hyphen, dot.
    /// Matches paths with optional ./ or ../ prefixes and / or \ separators.
    /// </summary>
    [GeneratedRegex(@"(?<![A-Za-z0-9_\-\.])(?:(?:\./)|(?:\.\./)+)?[A-Za-z0-9_\-\.]+(?:[\/\\][A-Za-z0-9_\-\.]+)*(?![A-Za-z0-9_\-\.])", RegexOptions.Compiled)]
    private static partial Regex RelativePathPattern();

    /// <inheritdoc/>
    public async IAsyncEnumerable<string> DetectAsync(IFile htmlSource, [EnumeratorCancellation] CancellationToken ct = default)
    {
        // Read HTML content
        using var stream = await htmlSource.OpenStreamAsync(FileAccess.Read, ct);
        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync(ct);

        // Find all matches
        var matches = RelativePathPattern().Matches(html);

        foreach (Match match in matches)
        {
            if (ct.IsCancellationRequested)
                yield break;

            var path = match.Value;

            // Filter out non-relative patterns
            if (string.IsNullOrWhiteSpace(path))
                continue;

            // Exclude absolute schemes
            if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("https://", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("//", StringComparison.Ordinal))
                continue;

            // Exclude absolute root paths (optional - treating these as non-relative)
            if (path.StartsWith('/') || path.StartsWith('\\'))
                continue;

            yield return path;
        }
    }
}
