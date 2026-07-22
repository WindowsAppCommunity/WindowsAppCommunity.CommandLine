using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.Pages;

/// <summary>
/// Maps a source markdown file to its generated folderized page route.
/// </summary>
/// <param name="SourceFile">The source markdown file.</param>
/// <param name="PageFolderPath">The generated page folder path relative to the site root.</param>
public sealed record MarkdownPageRoute(IFile SourceFile, string PageFolderPath)
{
    /// <summary>
    /// Gets the generated page route as a folder URL.
    /// </summary>
    public string PageUrlPath => string.IsNullOrWhiteSpace(PageFolderPath) ? "./" : $"{PageFolderPath.TrimEnd('/')}/";
}