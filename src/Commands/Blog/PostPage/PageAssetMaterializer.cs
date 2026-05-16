using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;

namespace WindowsAppCommunity.CommandLine.Blog.PostPage;

internal static class PageAssetMaterializer
{
    public static async Task<HashSet<string>> GetFileIdsAsync(IStorable source)
    {
        if (source is IFile file)
            return [file.Id];

        if (source is IFolder folder)
            return [.. await new DepthFirstRecursiveFolder(folder).GetFilesAsync().Select(x => x.Id).ToListAsync()];

        return [];
    }

    public static async Task CopyAssetsAsync(IModifiableFolder pageOutputFolder, IEnumerable<PageAsset> assets)
    {
        foreach (var asset in assets)
        {
            if (Path.GetExtension(asset.ResolvedFile.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
                continue;

            var rewrittenPath = NormalizePath(asset.RewrittenPath);
            var directoryPath = NormalizePath(Path.GetDirectoryName(rewrittenPath));
            var assetOutputFolder = pageOutputFolder;

            if (!string.IsNullOrWhiteSpace(directoryPath) && directoryPath != ".")
            {
                assetOutputFolder = (IModifiableFolder)await pageOutputFolder
                    .CreateFoldersAlongRelativePathAsync(directoryPath, overwrite: false)
                    .LastAsync();
            }

            await assetOutputFolder.CreateCopyOfAsync(asset.ResolvedFile, overwrite: true);
        }
    }

    private static string NormalizePath(string? path)
    {
        return path?.Replace('\\', '/') ?? string.Empty;
    }
}