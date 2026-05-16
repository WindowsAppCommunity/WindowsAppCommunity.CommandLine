using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.CommandLine;
using WindowsAppCommunity.CommandLine.Blog.PostPage;

namespace WindowsAppCommunity.CommandLine.Tests.Blog;

[TestClass]
public class BlogCommandMaterializationTests
{
    [TestMethod]
    public async Task PageCommand_CopiesGeneratedIndexAndTemplateAssets()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var markdownPath = Path.Combine(tempRoot, "post.md");
            var templateFolder = Path.Combine(tempRoot, "template");
            var outputFolder = Path.Combine(tempRoot, "output");

            Directory.CreateDirectory(Path.Combine(templateFolder, "images"));
            Directory.CreateDirectory(outputFolder);

            await File.WriteAllTextAsync(markdownPath, "---\ntitle: Test Post\n---\n\n# Hello");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "template.html"), "<html><head><link rel=\"stylesheet\" href=\"styles.css\"></head><body><img src=\"images/logo.png\">{{ body }}</body></html>");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "styles.css"), "body { color: black; }");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "images", "logo.png"), "logo");

            var exitCode = await new PageCommand().InvokeAsync([
                "--markdown", markdownPath,
                "--template", templateFolder,
                "--output", outputFolder]);

            var pageOutputFolder = Path.Combine(outputFolder, "post");

            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "index.html")), "index.html should be generated.");
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "styles.css")), "styles.css should be copied as a file.");
            Assert.IsFalse(Directory.Exists(Path.Combine(pageOutputFolder, "styles.css")), "styles.css must not be materialized as a folder.");
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "images", "logo.png")), "Nested template image should be copied.");
            Assert.IsTrue(File.Exists(Path.Combine(templateFolder, "styles.css")), "Template source asset should remain in place.");
            Assert.AreEqual(0, Directory.GetFiles(pageOutputFolder, "*.md", SearchOption.AllDirectories).Length, "Markdown source should not be copied into page output.");
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [TestMethod]
    public async Task PagesCommand_CopiesTemplateAssetsAndReferencedSourceAssetsToRewrittenPaths()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourceFolder = Path.Combine(tempRoot, "source");
            var templateFolder = Path.Combine(tempRoot, "template");
            var outputFolder = Path.Combine(tempRoot, "output");

            Directory.CreateDirectory(Path.Combine(sourceFolder, "images"));
            Directory.CreateDirectory(Path.Combine(templateFolder, "images"));
            Directory.CreateDirectory(outputFolder);

            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "page1.md"), "---\ntitle: Page 1\n---\n\n# Page 1\n\n![Content](images/content.png)");
            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "images", "content.png"), "content");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "template.html"), "<html><head><link rel=\"stylesheet\" href=\"styles.css\"></head><body><img src=\"images/template-logo.png\">{{ body }}</body></html>");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "styles.css"), "body { color: black; }");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "images", "template-logo.png"), "logo");

            var exitCode = await new PagesCommand().InvokeAsync([
                "--markdown-folder", sourceFolder,
                "--template", templateFolder,
                "--output", outputFolder]);

            var pageOutputFolder = Path.Combine(outputFolder, "page1");

            Assert.AreEqual(0, exitCode);
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "index.html")), "Page index.html should be generated.");
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "styles.css")), "Template stylesheet should be copied into each page folder.");
            Assert.IsFalse(Directory.Exists(Path.Combine(pageOutputFolder, "styles.css")), "styles.css must not be materialized as a folder.");
            Assert.IsTrue(File.Exists(Path.Combine(pageOutputFolder, "images", "template-logo.png")), "Template image should be copied into each page folder.");
            Assert.IsTrue(File.Exists(Path.Combine(outputFolder, "images", "content.png")), "Referenced source image should be copied to the rewritten parent-relative path.");
            Assert.AreEqual(0, Directory.GetFiles(outputFolder, "*.md", SearchOption.AllDirectories).Length, "Markdown source should not be copied into multi-page output.");
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [TestMethod]
    public async Task PagesCommand_RewritesInRootMarkdownLinksToGeneratedPageRoutes()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourceFolder = Path.Combine(tempRoot, "source");
            var templateFolder = Path.Combine(tempRoot, "template");
            var outputFolder = Path.Combine(tempRoot, "output");

            Directory.CreateDirectory(Path.Combine(sourceFolder, "sub"));
            Directory.CreateDirectory(templateFolder);
            Directory.CreateDirectory(outputFolder);

            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "page1.md"), "---\ntitle: Page 1\n---\n\n[Page 2](page2.md)\n\n[Page 3](sub/page3.md)");
            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "page2.md"), "---\ntitle: Page 2\n---\n\n[Page 1](page1.md)");
            await File.WriteAllTextAsync(Path.Combine(sourceFolder, "sub", "page3.md"), "---\ntitle: Page 3\n---\n\n[Page 1](../page1.md)");
            await File.WriteAllTextAsync(Path.Combine(templateFolder, "template.html"), "<html><body>{{ body }}</body></html>");

            var exitCode = await new PagesCommand().InvokeAsync([
                "--markdown-folder", sourceFolder,
                "--template", templateFolder,
                "--output", outputFolder]);

            var page1Html = await File.ReadAllTextAsync(Path.Combine(outputFolder, "page1", "index.html"));
            var page2Html = await File.ReadAllTextAsync(Path.Combine(outputFolder, "page2", "index.html"));
            var page3Html = await File.ReadAllTextAsync(Path.Combine(outputFolder, "sub", "page3", "index.html"));

            Assert.AreEqual(0, exitCode);
            StringAssert.Contains(page1Html, "href=\"../page2/\"");
            StringAssert.Contains(page1Html, "href=\"../sub/page3/\"");
            StringAssert.Contains(page2Html, "href=\"../page1/\"");
            StringAssert.Contains(page3Html, "href=\"../../page1/\"");
            Assert.IsFalse(page1Html.Contains(".md"), "Generated page1 HTML should not link to raw markdown files.");
            Assert.IsFalse(page2Html.Contains(".md"), "Generated page2 HTML should not link to raw markdown files.");
            Assert.IsFalse(page3Html.Contains(".md"), "Generated page3 HTML should not link to raw markdown files.");
            Assert.AreEqual(0, Directory.GetFiles(outputFolder, "*.md", SearchOption.AllDirectories).Length, "Markdown source should not be copied into multi-page output.");
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "wac-blog-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private static void DeleteTempRoot(string tempRoot)
    {
        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, recursive: true);
    }
}