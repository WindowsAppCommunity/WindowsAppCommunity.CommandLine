using System.Runtime.CompilerServices;
using OwlCore.Storage;
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
        var routesByFileId = new Dictionary<string, MarkdownPageRoute>();
        await AddFolderRoutesAsync(markdownSourceFolder, string.Empty, routesByFileId, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        await foreach (var item in folder.GetItemsAsync(StorableType.All, cancellationToken).WithCancellation(cancellationToken))
        {
            if (item is IFile file && Path.GetExtension(file.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
            {
                var pageFolderName = HtmlTemplatedMarkdownPageFolder.GetPageFolderName(file.Name);
                var pageFolderPath = CombineRoutePath(currentFolderPath, pageFolderName);
                routesByFileId[file.Id] = new MarkdownPageRoute(file, pageFolderPath);
            }

            if (item is IFolder subfolder)
            {
                var nestedFolderPath = CombineRoutePath(currentFolderPath, subfolder.Name);
                await AddFolderRoutesAsync(subfolder, nestedFolderPath, routesByFileId, cancellationToken);
            }
        }
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