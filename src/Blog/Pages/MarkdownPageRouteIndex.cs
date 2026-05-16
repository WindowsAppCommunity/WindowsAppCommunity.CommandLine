using OwlCore.Extensions;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Page;

namespace WindowsAppCommunity.Blog.Pages;

/// <summary>
/// Source-derived route index for folderized markdown pages.
/// </summary>
public sealed class MarkdownPageRouteIndex
{
    private readonly Dictionary<string, MarkdownPageRoute> _routesByFileId;

    private MarkdownPageRouteIndex(Dictionary<string, MarkdownPageRoute> routesByFileId)
    {
        _routesByFileId = routesByFileId;
    }

    /// <summary>
    /// Gets all indexed markdown page routes.
    /// </summary>
    public IReadOnlyCollection<MarkdownPageRoute> Routes => _routesByFileId.Values;

    /// <summary>
    /// Creates an index from the markdown source folder tree.
    /// </summary>
    public static async Task<MarkdownPageRouteIndex> CreateAsync(IFolder markdownSourceFolder, CancellationToken cancellationToken = default)
    {
        return await CreateAsync(markdownSourceFolder, null, null, cancellationToken);
    }

    /// <summary>
    /// Creates an index from the markdown source folder tree and recursively discovered markdown links.
    /// </summary>
    public static async Task<MarkdownPageRouteIndex> CreateAsync(
        IFolder markdownSourceFolder,
        IAssetLinkDetector? linkDetector,
        IAssetResolver? resolver,
        CancellationToken cancellationToken = default)
    {
        var routesByFileId = new Dictionary<string, MarkdownPageRoute>();
        var pendingFiles = new Queue<IFile>();
        await AddFolderRoutesAsync(markdownSourceFolder, string.Empty, routesByFileId, pendingFiles, cancellationToken);

        if (linkDetector is not null && resolver is not null)
            await AddLinkedMarkdownRoutesAsync(linkDetector, resolver, routesByFileId, pendingFiles, cancellationToken);

        return new MarkdownPageRouteIndex(routesByFileId);
    }

    /// <summary>
    /// Attempts to get the generated route for a source markdown file.
    /// </summary>
    public bool TryGetRoute(IFile sourceMarkdownFile, out MarkdownPageRoute? route)
    {
        return _routesByFileId.TryGetValue(sourceMarkdownFile.Id, out route);
    }

    /// <summary>
    /// Attempts to get a generated page route relative from the referencing markdown page route.
    /// </summary>
    public bool TryGetRelativeRoute(IFile referencingMarkdownFile, IFile referencedMarkdownFile, out string? relativeRoute)
    {
        relativeRoute = null;

        if (!TryGetRoute(referencingMarkdownFile, out var referencingRoute) || referencingRoute is null)
            return false;

        if (!TryGetRoute(referencedMarkdownFile, out var referencedRoute) || referencedRoute is null)
            return false;

        relativeRoute = GetRelativeFolderRoute(referencingRoute.PageFolderPath, referencedRoute.PageFolderPath);
        return true;
    }

    private static async Task AddFolderRoutesAsync(
        IFolder folder,
        string currentFolderPath,
        Dictionary<string, MarkdownPageRoute> routesByFileId,
        Queue<IFile> pendingFiles,
        CancellationToken cancellationToken)
    {
        await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken).WithCancellation(cancellationToken))
        {
            if (item is IFile file && Path.GetExtension(file.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                var pageFolderName = HtmlTemplatedMarkdownPageFolder.GetPageFolderName(file.Name);
                var pageFolderPath = CombineRoutePath(currentFolderPath, pageFolderName);
                AddRoute(routesByFileId, pendingFiles, file, pageFolderPath);
            }

            if (item is IFolder subfolder)
            {
                var nestedFolderPath = CombineRoutePath(currentFolderPath, subfolder.Name);
                await AddFolderRoutesAsync(subfolder, nestedFolderPath, routesByFileId, pendingFiles, cancellationToken);
            }
        }
    }

    private static async Task AddLinkedMarkdownRoutesAsync(
        IAssetLinkDetector linkDetector,
        IAssetResolver resolver,
        Dictionary<string, MarkdownPageRoute> routesByFileId,
        Queue<IFile> pendingFiles,
        CancellationToken cancellationToken)
    {
        while (pendingFiles.Count > 0)
        {
            var currentFile = pendingFiles.Dequeue();

            await foreach (var link in linkDetector.DetectAsync(currentFile, cancellationToken).WithCancellation(cancellationToken))
            {
                var resolvedFile = await resolver.ResolveAsync(currentFile, link, cancellationToken);
                if (resolvedFile is null)
                    continue;

                if (!Path.GetExtension(resolvedFile.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (routesByFileId.ContainsKey(resolvedFile.Id))
                    continue;

                var externalRoutePath = CombineRoutePath("_linked", resolvedFile.Id.HashMD5Fast());
                AddRoute(routesByFileId, pendingFiles, resolvedFile, externalRoutePath);
            }
        }
    }

    private static void AddRoute(Dictionary<string, MarkdownPageRoute> routesByFileId, Queue<IFile> pendingFiles, IFile file, string pageFolderPath)
    {
        routesByFileId[file.Id] = new MarkdownPageRoute(file, pageFolderPath);
        pendingFiles.Enqueue(file);
    }

    private static string CombineRoutePath(string parentPath, string childName)
    {
        return string.IsNullOrWhiteSpace(parentPath) ? childName : $"{parentPath.TrimEnd('/')}/{childName}";
    }

    private static string GetRelativeFolderRoute(string fromPageFolderPath, string toPageFolderPath)
    {
        var fromSegments = SplitRoutePath(fromPageFolderPath).ToArray();
        var toSegments = SplitRoutePath(toPageFolderPath).ToArray();

        var commonLength = 0;
        while (commonLength < fromSegments.Length &&
               commonLength < toSegments.Length &&
               string.Equals(fromSegments[commonLength], toSegments[commonLength], StringComparison.OrdinalIgnoreCase))
        {
            commonLength++;
        }

        var relativeSegments = Enumerable
            .Repeat("..", fromSegments.Length - commonLength)
            .Concat(toSegments.Skip(commonLength))
            .ToArray();

        if (relativeSegments.Length == 0)
            return "./";

        return $"{string.Join('/', relativeSegments)}/";
    }

    private static IEnumerable<string> SplitRoutePath(string routePath)
    {
        return routePath
            .Replace('\\', '/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}