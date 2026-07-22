using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Pages;

namespace WindowsAppCommunity.CommandLine.Tests.Blog;

[TestClass]
public class MarkdownPageRouteIndexTests
{
    [TestMethod]
    public async Task CreateAsync_IndexesFolderizedMarkdownRoutes()
    {
        var sourceFolder = new MemoryFolder("source", "source");
        var rootPage = await CreateFileAsync(sourceFolder, "page one.md");
        var nestedPage = await CreateFileAsync(sourceFolder, "area/child.md");

        var routeIndex = await MarkdownPageRouteIndex.CreateAsync(sourceFolder);

        Assert.IsTrue(routeIndex.TryGetRoute(rootPage, out var rootRoute));
        Assert.IsNotNull(rootRoute);
        Assert.AreEqual("page one", rootRoute.PageFolderPath);
        Assert.AreEqual("page one/", rootRoute.PageUrlPath);

        Assert.IsTrue(routeIndex.TryGetRoute(nestedPage, out var nestedRoute));
        Assert.IsNotNull(nestedRoute);
        Assert.AreEqual("area/child", nestedRoute.PageFolderPath);
        Assert.AreEqual("area/child/", nestedRoute.PageUrlPath);
    }

    [TestMethod]
    public async Task MarkdownPageAssetStrategy_RewritesMarkdownLinksToGeneratedPageRoutes()
    {
        var sourceFolder = new MemoryFolder("source", "source");
        var page1 = await CreateFileAsync(sourceFolder, "page1.md");
        var page2 = await CreateFileAsync(sourceFolder, "page2.md");
        var nestedPage = await CreateFileAsync(sourceFolder, "sub/page3.md");
        var image = await CreateFileAsync(sourceFolder, "images/logo.png");
        var routeIndex = await MarkdownPageRouteIndex.CreateAsync(sourceFolder);
        var strategy = new MarkdownPageAssetStrategy
        {
            RouteIndex = routeIndex,
            AssetStrategy = new KnownAssetStrategy
            {
                ReferencedAssetFileIds = [image.Id],
                UnknownAssetFallbackStrategy = AssetFallbackBehavior.Drop,
                UnknownAssetFaultStrategy = FaultStrategy.None,
            }
        };

        var siblingRoute = await strategy.DecideAsync(page1, page2, "page2.md");
        var childRoute = await strategy.DecideAsync(page1, nestedPage, "sub/page3.md");
        var parentRoute = await strategy.DecideAsync(nestedPage, page1, "../page1.md");
        var imageRoute = await strategy.DecideAsync(page1, image, "images/logo.png");

        Assert.AreEqual("../page2/", siblingRoute);
        Assert.AreEqual("../sub/page3/", childRoute);
        Assert.AreEqual("../../page1/", parentRoute);
        Assert.AreEqual("../images/logo.png", imageRoute);
    }

    [TestMethod]
    public async Task CreateAsync_WithDetectorAndResolver_IndexesLinkedMarkdownOutsideSourceRoot()
    {
        var notesRoot = new MemoryFolder("notes", "notes");
        var sourcePage = await CreateFileAsync(notesRoot, "current/page1.md", "[Outside](../linked/outside.md)");
        var outsidePage = await CreateFileAsync(notesRoot, "linked/outside.md", "[Page 1](../current/page1.md)");
        var sourceFolder = await notesRoot.GetFirstByNameAsync("current") as IFolder
            ?? throw new InvalidOperationException("Failed to get source folder");

        var routeIndex = await MarkdownPageRouteIndex.CreateAsync(sourceFolder, new RegexAssetLinkDetector(), new RelativePathAssetResolver());

        Assert.IsTrue(routeIndex.TryGetRoute(sourcePage, out var sourceRoute));
        Assert.IsNotNull(sourceRoute);
        Assert.AreEqual("page1", sourceRoute.PageFolderPath);

        Assert.IsTrue(routeIndex.TryGetRoute(outsidePage, out var outsideRoute));
        Assert.IsNotNull(outsideRoute);
        StringAssert.StartsWith(outsideRoute.PageFolderPath, "_linked/");

        Assert.IsTrue(routeIndex.TryGetRelativeRoute(sourcePage, outsidePage, out var relativeRoute));
        Assert.IsNotNull(relativeRoute);
        StringAssert.StartsWith(relativeRoute, "../_linked/");
    }

    private static async Task<IFile> CreateFileAsync(MemoryFolder folder, string relativePath)
    {
        return await CreateFileAsync(folder, relativePath, string.Empty);
    }

    private static async Task<IFile> CreateFileAsync(MemoryFolder folder, string relativePath, string content)
    {
        var file = await folder.CreateAlongRelativePathAsync(relativePath, StorableType.File).LastAsync() as IFile
            ?? throw new InvalidOperationException($"Failed to create {relativePath}");

        await file.WriteTextAsync(content);
        return file;
    }
}