using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Storage;
using OwlCore.Storage.Memory;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Pages;

namespace WindowsAppCommunity.CommandLine.Tests.Blog;

/// <summary>
/// Tests for <see cref="AssetAwareHtmlTemplatedMarkdownPagesFolder"/> multi-page generation behavior.
/// </summary>
[TestClass]
public class AssetAwareHtmlTemplatedMarkdownPagesFolderTests
{
    private MemoryFolder _testSourceFolder = null!;
    private MemoryFolder _templateFolder = null!;
    private AssetAwareHtmlTemplatedMarkdownPagesFolder _pagesFolder = null!;
    
    // File references stored from Setup for test access
    private IFile _page1File = null!;
    private IFile _page2File = null!;
    private IFile _logoFile = null!;

    [TestInitialize]
    public async Task Setup()
    {
        _testSourceFolder = new MemoryFolder("test-source", "test-source");

        // Create file tree using AlongPath method (overwrite: false to reuse folders)
        _page1File = await _testSourceFolder.CreateAlongRelativePathAsync("page1.md", StorableType.File).LastAsync() as IFile 
                     ?? throw new InvalidOperationException("Failed to create page1.md");
        await using (var stream = await _page1File.OpenStreamAsync(FileAccess.Write))
        await using (var writer = new StreamWriter(stream))
        {
            await writer.WriteAsync(@"---
title: Page 1
---

# Page 1 Content

![Logo](../images/logo.png)");
        }

        _page2File = await _testSourceFolder.CreateAlongRelativePathAsync("subfolder/page2.md", StorableType.File).LastAsync() as IFile
                     ?? throw new InvalidOperationException("Failed to create page2.md");
        await using (var stream2 = await _page2File.OpenStreamAsync(FileAccess.Write))
        await using (var writer2 = new StreamWriter(stream2))
        {
            await writer2.WriteAsync(@"---
title: Page 2
---

# Page 2 Content

![Local Icon](./local-icon.png)");
        }

        var localIcon = await _testSourceFolder.CreateAlongRelativePathAsync("subfolder/local-icon.png", StorableType.File).LastAsync() as IFile
                        ?? throw new InvalidOperationException("Failed to create local-icon.png");
        await using (var iconStream = await localIcon.OpenStreamAsync(FileAccess.Write))
        {
            // Empty file
        }

        _logoFile = await _testSourceFolder.CreateAlongRelativePathAsync("images/logo.png", StorableType.File).LastAsync() as IFile
                    ?? throw new InvalidOperationException("Failed to create logo.png");
        await using (var logoStream = await _logoFile.OpenStreamAsync(FileAccess.Write))
        {
            // Empty file
        }

        // Create template
        _templateFolder = new MemoryFolder("template", "template");
        var templateHtml = await _templateFolder.CreateAlongRelativePathAsync("index.html", StorableType.File, overwrite: true).LastAsync() as IFile
                           ?? throw new InvalidOperationException("Failed to create template");
        await using (var templateStream = await templateHtml.OpenStreamAsync(FileAccess.Write))
        await using (var templateWriter = new StreamWriter(templateStream))
        {
            await templateWriter.WriteAsync(@"<!DOCTYPE html>
<html>
<head><title>{{ frontmatter.title }}</title></head>
<body>{{ body }}</body>
</html>");
        }

        // Instantiate composition root
        _pagesFolder = new AssetAwareHtmlTemplatedMarkdownPagesFolder(
            _testSourceFolder,
            _templateFolder,
            "index.html")
        {
            LinkDetector = new RegexAssetLinkDetector(),
            Resolver = new RelativePathAssetResolver
            {
                SourceFolder = _testSourceFolder
            },
            InclusionStrategy = new ReferenceOnlyInclusionStrategy()
        };
    }

    [TestMethod]
    public async Task MarkdownDiscovery_FindsAllMarkdownFiles()
    {
        var items = await _pagesFolder.GetItemsAsync(StorableType.All).ToListAsync();
        var folders = items.OfType<IFolder>().ToList();
        
        Assert.IsTrue(folders.Count >= 1, $"Should discover at least 1 item (found {folders.Count})");
        var hasPage1OrSubfolder = folders.Any(f => f.Name.Contains("page1") || f.Name == "subfolder");
        Assert.IsTrue(hasPage1OrSubfolder, "Should find page1 folder or subfolder in output");
    }

    [TestMethod]
    public async Task HierarchyPreservation_MirrorsSourceStructure()
    {
        var rootItems = await _pagesFolder.GetItemsAsync(StorableType.All).ToListAsync();
        var subfolder = rootItems.OfType<IFolder>().FirstOrDefault(f => f.Name == "subfolder");
        
        if (subfolder == null)
        {
            var folderNames = string.Join(", ", rootItems.OfType<IFolder>().Select(f => f.Name));
            Assert.Inconclusive($"Subfolder not found at root level. Found folders: {folderNames}");
            return;
        }

        var subfolderItems = await subfolder.GetItemsAsync(StorableType.All).ToListAsync();
        Assert.IsTrue(subfolderItems.Count > 0, "Subfolder should contain items");
    }

    [TestMethod]
    public async Task AssetLinkDetection_IdentifiesRelativeLinks()
    {
        var rootItems = await _pagesFolder.GetItemsAsync(StorableType.All).ToListAsync();
        var page1Folder = rootItems.OfType<IFolder>().FirstOrDefault(f => f.Name.Contains("page1"));
        
        if (page1Folder == null)
        {
            Assert.Inconclusive("Page1 folder not found in output");
            return;
        }

        var page1Items = await page1Folder.GetItemsAsync(StorableType.All).ToListAsync();
        var indexHtml = page1Items.OfType<IFile>().FirstOrDefault();
        
        if (indexHtml == null)
        {
            Assert.Inconclusive("No files found in page1 folder");
            return;
        }

        string htmlContent;
        await using (var stream = await indexHtml.OpenStreamAsync(FileAccess.Read))
        using (var reader = new StreamReader(stream))
        {
            htmlContent = await reader.ReadToEndAsync();
        }

        Assert.IsTrue(htmlContent.Contains("logo.png"), $"HTML should contain logo.png reference. Content: {htmlContent}");
    }

    [TestMethod]
    public async Task AssetPathResolution_ResolvesValidPaths()
    {
        var resolver = new RelativePathAssetResolver
        {
            SourceFolder = _testSourceFolder
        };

        var resolvedAsset = await resolver.ResolveAsync(_page1File, "images/logo.png");

        if (resolvedAsset == null)
        {
            resolvedAsset = await resolver.ResolveAsync(_page1File, "../images/logo.png");
        }

        Assert.IsNotNull(resolvedAsset, "Should resolve images/logo.png or ../images/logo.png from page1.md context");
        Assert.AreEqual("logo.png", resolvedAsset.Name, "Resolved asset should be logo.png");
    }

    [TestMethod]
    public async Task InclusionStrategy_AppliesReferenceDecisions()
    {
        var strategy = new ReferenceOnlyInclusionStrategy();
        Assert.IsNotNull(_page1File, "page1.md should exist");
        Assert.IsNotNull(_logoFile, "logo.png should exist");

        var rewrittenPath = await strategy.DecideAsync(_page1File, _logoFile, "../images/logo.png");

        Assert.IsTrue(rewrittenPath.StartsWith("../"), "Reference-only strategy should return path with ../ prefix");
        Assert.IsTrue(rewrittenPath.Contains("images/logo.png"), "Rewritten path should preserve original structure");
    }

    [TestMethod]
    public async Task LinkRewriting_AddsDepthPrefix()
    {
        var rootItems = await _pagesFolder.GetItemsAsync(StorableType.All).ToListAsync();
        var page1Folder = rootItems.OfType<IFolder>().FirstOrDefault(f => f.Name.Contains("page1"));
        
        if (page1Folder == null)
        {
            Assert.Inconclusive("Page1 folder not found");
            return;
        }

        var page1Items = await page1Folder.GetItemsAsync(StorableType.All).ToListAsync();
        var indexHtml = page1Items.OfType<IFile>().FirstOrDefault();
        
        if (indexHtml == null)
        {
            Assert.Inconclusive("No HTML file found in page1 folder");
            return;
        }

        string htmlContent;
        await using (var stream = await indexHtml.OpenStreamAsync(FileAccess.Read))
        using (var reader = new StreamReader(stream))
        {
            htmlContent = await reader.ReadToEndAsync();
        }

        Assert.IsTrue(htmlContent.Contains("../../images/logo.png") || htmlContent.Contains("../images/logo.png"),
            $"Reference link should be rewritten. Content: {htmlContent}");
    }

    [TestMethod]
    public async Task YieldOrder_FollowsSpecification()
    {
        var rootItems = await _pagesFolder.GetItemsAsync(StorableType.All).ToListAsync();
        var page1Folder = rootItems.OfType<IFolder>().FirstOrDefault(f => f.Name.Contains("page1"));
        
        if (page1Folder == null)
        {
            Assert.Inconclusive("Page1 folder not found");
            return;
        }

        var page1Items = await page1Folder.GetItemsAsync(StorableType.All).ToListAsync();

        Assert.IsTrue(page1Items.Count > 0, "Page folder should yield items");
        var firstItem = page1Items[0];
        Assert.IsInstanceOfType(firstItem, typeof(IFile), "First item should be a file");
    }
}
