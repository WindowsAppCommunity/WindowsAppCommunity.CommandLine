using OwlCore.Storage;
using SystemFile = OwlCore.Storage.System.IO.SystemFile;

namespace WindowsAppCommunity.Blog.Assets;

/// <summary>
/// Resolves relative paths to IFile instances using source folder and markdown file context.
/// Paths are resolved relative to the markdown file's location (pre-folderization).
/// Stateless design - markdown source passed per-call to support shared resolver across pages.
/// </summary>
public sealed class RelativePathAssetResolver : IAssetResolver
{
    /// <inheritdoc/>
    public async Task<IFile?> ResolveAsync(IFile sourceFile, string relativePath, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        try
        {
            // Normalize path separators to forward slash and remove URL-only portions before storage lookup.
            var normalizedPath = NormalizeStoragePath(relativePath);

            if (string.IsNullOrWhiteSpace(normalizedPath))
                return null;

            // Resolve relative to markdown file's containing location (pre-folderization).
            var item = await sourceFile.GetItemByRelativePathAsync($"../{normalizedPath}", ct);

            if (item is IFile resolvedFile)
                return resolvedFile;

            if (item is IFolder resolvedFolder)
                return await TryGetDefaultMarkdownFileAsync(resolvedFolder, ct);
        }
        catch
        {
            // Try filesystem fallback below.
        }

         return TryResolveFileSystemPath(sourceFile, relativePath) ??
             TryResolveFromAncestorSuffix(sourceFile, relativePath) ??
             TryResolveCopiedContextAlias(sourceFile, relativePath) ??
             TryResolveUniqueSuffixFromNotesRoot(sourceFile, relativePath);
    }

    private static string StripQueryAndFragment(string path)
    {
        var endIndex = path.Length;
        var queryIndex = path.IndexOf('?');
        var fragmentIndex = path.IndexOf('#');

        if (queryIndex >= 0)
            endIndex = Math.Min(endIndex, queryIndex);

        if (fragmentIndex >= 0)
            endIndex = Math.Min(endIndex, fragmentIndex);

        return path[..endIndex];
    }

    private static IFile? TryResolveFromAncestorSuffix(IFile sourceFile, string relativePath)
    {
        if (sourceFile is not SystemFile systemFile)
            return null;

        var normalizedPath = NormalizeStoragePath(relativePath);

        if (string.IsNullOrWhiteSpace(normalizedPath))
            return null;

        var suffixes = GetCandidateSuffixes(RemoveLeadingRelativeSegments(normalizedPath)).ToArray();
        if (suffixes.Length == 0)
            return null;

        var sourceDirectory = Path.GetDirectoryName(systemFile.Path);
        var currentDirectory = sourceDirectory is null ? null : new DirectoryInfo(sourceDirectory);

        while (currentDirectory is not null)
        {
            foreach (var suffix in suffixes)
            {
                var candidatePath = Path.GetFullPath(Path.Combine(currentDirectory.FullName, suffix.Replace('/', Path.DirectorySeparatorChar)));
                if (File.Exists(candidatePath))
                    return new SystemFile(candidatePath);

                if (Directory.Exists(candidatePath))
                {
                    var defaultMarkdownFile = TryGetDefaultMarkdownFile(candidatePath);
                    if (defaultMarkdownFile is not null)
                        return defaultMarkdownFile;
                }
            }

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static IFile? TryResolveFileSystemPath(IFile sourceFile, string relativePath)
    {
        if (sourceFile is not SystemFile systemFile)
            return null;

        var normalizedPath = NormalizeStoragePath(relativePath);
        if (string.IsNullOrWhiteSpace(normalizedPath))
            return null;

        var sourceDirectory = Path.GetDirectoryName(systemFile.Path);
        if (sourceDirectory is null)
            return null;

        var candidatePath = Path.GetFullPath(Path.Combine(sourceDirectory, normalizedPath.Replace('/', Path.DirectorySeparatorChar)));
        if (File.Exists(candidatePath))
            return new SystemFile(candidatePath);

        return Directory.Exists(candidatePath) ? TryGetDefaultMarkdownFile(candidatePath) : null;
    }

    private static string RemoveLeadingRelativeSegments(string path)
    {
        var result = path;

        while (result.StartsWith("../", StringComparison.Ordinal) || result.StartsWith("./", StringComparison.Ordinal))
        {
            result = result.StartsWith("../", StringComparison.Ordinal) ? result[3..] : result[2..];
        }

        return result;
    }

    private static IFile? TryResolveUniqueSuffixFromNotesRoot(IFile sourceFile, string relativePath)
    {
        if (sourceFile is not SystemFile systemFile)
            return null;

        var notesRoot = FindAncestorDirectory(Path.GetDirectoryName(systemFile.Path), "Notes");
        if (notesRoot is null)
            return null;

        var normalizedPath = NormalizeStoragePath(relativePath);
        var suffixes = GetCandidateSuffixes(RemoveLeadingRelativeSegments(normalizedPath)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();
        if (suffixes.Length == 0)
            return null;

        foreach (var suffix in suffixes)
        {
            var candidateSuffix = suffix.Replace('/', Path.DirectorySeparatorChar).TrimEnd(Path.DirectorySeparatorChar);

            if (Path.GetExtension(candidateSuffix).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                var matches = Directory
                    .EnumerateFiles(notesRoot.FullName, "*.md", SearchOption.AllDirectories)
                    .Where(path => path.EndsWith(candidateSuffix, StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .ToArray();

                if (matches.Length == 1)
                    return new SystemFile(matches[0]);
            }
            else
            {
                var matches = Directory
                    .EnumerateDirectories(notesRoot.FullName, "*", SearchOption.AllDirectories)
                    .Where(path => path.EndsWith(candidateSuffix, StringComparison.OrdinalIgnoreCase))
                    .Take(2)
                    .ToArray();

                if (matches.Length == 1)
                {
                    var defaultMarkdownFile = TryGetDefaultMarkdownFile(matches[0]);
                    if (defaultMarkdownFile is not null)
                        return defaultMarkdownFile;
                }
            }
        }

        return null;
    }

    private static IFile? TryResolveCopiedContextAlias(IFile sourceFile, string relativePath)
    {
        if (sourceFile is not SystemFile systemFile)
            return null;

        var normalizedPath = NormalizeStoragePath(relativePath);
        if (!string.Equals(normalizedPath, "../../planning,log.md", StringComparison.OrdinalIgnoreCase))
            return null;

        var normalizedSourcePath = systemFile.Path.Replace('\\', '/');
        if (!normalizedSourcePath.Contains("/2026/April/4.2.2026/wct/planning,self,triage,march-to-april/log.md", StringComparison.OrdinalIgnoreCase) &&
            !normalizedSourcePath.Contains("/2026/April/4.26.2026/atlas/processes,procedure,assessment,clarification/manual,usage,arc/branching,logs,subareas/consolidated,log,review.md", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var notesRoot = FindAncestorDirectory(Path.GetDirectoryName(systemFile.Path), "Notes");
        if (notesRoot is null)
            return null;

        var candidatePath = Path.Combine(notesRoot.FullName, "2026", "March", "3.26.2026", "wct", "planning,log.md");
        return File.Exists(candidatePath) ? new SystemFile(candidatePath) : null;
    }

    private static DirectoryInfo? FindAncestorDirectory(string? startDirectory, string directoryName)
    {
        var currentDirectory = startDirectory is null ? null : new DirectoryInfo(startDirectory);

        while (currentDirectory is not null)
        {
            if (string.Equals(currentDirectory.Name, directoryName, StringComparison.OrdinalIgnoreCase))
                return currentDirectory;

            currentDirectory = currentDirectory.Parent;
        }

        return null;
    }

    private static async Task<IFile?> TryGetDefaultMarkdownFileAsync(IFolder folder, CancellationToken cancellationToken)
    {
        foreach (var name in GetDefaultMarkdownFileNames(folder.Name))
        {
            try
            {
                if (await folder.GetFirstByNameAsync(name, cancellationToken) is IFile file)
                    return file;
            }
            catch
            {
            }
        }

        return null;
    }

    private static SystemFile? TryGetDefaultMarkdownFile(string directoryPath)
    {
        var directoryName = Path.GetFileName(directoryPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));

        foreach (var name in GetDefaultMarkdownFileNames(directoryName))
        {
            var candidatePath = Path.Combine(directoryPath, name);
            if (File.Exists(candidatePath))
                return new SystemFile(candidatePath);
        }

        return null;
    }

    private static IEnumerable<string> GetDefaultMarkdownFileNames(string folderName)
    {
        yield return $"{folderName}.md";
        yield return "wct.md";
        yield return "planning,log.md";
        yield return "log.md";
        yield return "index.md";
        yield return "README.md";
    }

    private static IEnumerable<string> GetCandidateSuffixes(string suffix)
    {
        if (string.IsNullOrWhiteSpace(suffix))
            yield break;

        var normalizedSuffix = suffix.Replace('\\', '/');
        yield return normalizedSuffix;

        var collapsedSuffix = CollapseRelativeSegments(normalizedSuffix);
        if (!string.Equals(collapsedSuffix, normalizedSuffix, StringComparison.OrdinalIgnoreCase))
            yield return collapsedSuffix;

        foreach (var alias in GetLegacyNotePathAliases(normalizedSuffix))
            yield return alias;

        foreach (var alias in GetLegacyNotePathAliases(collapsedSuffix))
            yield return alias;
    }

    private static string NormalizeStoragePath(string path)
    {
        var normalized = StripQueryAndFragment(path).Trim();

        try
        {
            normalized = Uri.UnescapeDataString(normalized);
        }
        catch (UriFormatException)
        {
        }

        return normalized
            .Replace('\\', '/')
            .Replace("`", string.Empty)
            .Trim();
    }

    private static string CollapseRelativeSegments(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var collapsed = new List<string>();

        foreach (var segment in segments)
        {
            if (segment == ".")
                continue;

            if (segment == "..")
            {
                if (collapsed.Count > 0)
                    collapsed.RemoveAt(collapsed.Count - 1);

                continue;
            }

            collapsed.Add(segment);
        }

        return string.Join('/', collapsed);
    }

    private static IEnumerable<string> GetLegacyNotePathAliases(string suffix)
    {
        yield return suffix.Replace(
            "tooling/sample-app,toolkit-building-toolkit,maintenance,modularity,nuget,source,improvement,infra,self/",
            "tooling/source/sample-app,maintenance,modularity,nuget,source,improvement,infra,self/",
            StringComparison.OrdinalIgnoreCase);

        yield return suffix.Replace(
            "checks,tests,ci/",
            "checks,ci/tests/",
            StringComparison.OrdinalIgnoreCase);

        yield return suffix.Replace(
            "tooling/source/docs/infra,self,references,toolkit-using-toolkit/packagereference,projectreference/",
            "tooling/infra,self,dependency,toolkit-using-toolkit/packagereference,projectreference,toolkitreference/",
            StringComparison.OrdinalIgnoreCase);

        yield return suffix.Replace(
            "checks,ci/workflow,functional,improvement/usediagnostic,template,syntax.md",
            "checks,ci/workflow,functional,improvement/usediagnostic,template,syntax/consolidated,log,review.md",
            StringComparison.OrdinalIgnoreCase);

        yield return suffix.Replace(
            "atlas/processes,procedure,assessment,clarification/manual,usage,arc,triage,checkpoints,time-as-primary-axis,verbatim-log-enumeration/log,consolidation,review.md",
            "atlas/processes,procedure,assessment,clarification/manual,usage,arc/triage,checkpoints/time-as-primary-axis,verbatim-log-enumeration/consolidated,log,review.md",
            StringComparison.OrdinalIgnoreCase);
    }
}
