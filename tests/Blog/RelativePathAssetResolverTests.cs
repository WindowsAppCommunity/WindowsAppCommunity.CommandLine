using Microsoft.VisualStudio.TestTools.UnitTesting;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.Blog.Assets;

namespace WindowsAppCommunity.CommandLine.Tests.Blog;

[TestClass]
public class RelativePathAssetResolverTests
{
    [TestMethod]
    public async Task ResolveAsync_UsesDefaultMarkdownFileForDirectoryLinks()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourcePath = Path.Combine(tempRoot, "Notes", "2026", "March", "3.16.2026", "wct.md");
            var targetPath = Path.Combine(tempRoot, "Notes", "2026", "March", "3.2.2026", "wct", "planning,log.md");
            await CreateTextFileAsync(sourcePath);
            await CreateTextFileAsync(targetPath);

            var resolvedFile = await new RelativePathAssetResolver().ResolveAsync(new SystemFile(sourcePath), "../3.2.2026/wct/");

            Assert.IsNotNull(resolvedFile);
            Assert.AreEqual(targetPath, ((SystemFile)resolvedFile).Path);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [TestMethod]
    public async Task ResolveAsync_UsesUniqueNotesRootSuffixForCopiedLegacyLinks()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourcePath = Path.Combine(tempRoot, "Notes", "2026", "April", "4.26.2026", "wct", "triage", "self", "4.2.2026", "toc,outline,evaluation", "log.md");
            var targetPath = Path.Combine(tempRoot, "Notes", "2026", "March", "3.24.2026", "wct", "winui,wasdk", "1.8,upgrade", "planning,log.md");
            await CreateTextFileAsync(sourcePath);
            await CreateTextFileAsync(targetPath);

            var resolvedFile = await new RelativePathAssetResolver().ResolveAsync(new SystemFile(sourcePath), "../../../../../../%60March/3.24.2026%5Cwct%5Cwinui,wasdk%5C1.8,upgrade%5Cplanning,log.md");

            Assert.IsNotNull(resolvedFile);
            Assert.AreEqual(targetPath, ((SystemFile)resolvedFile).Path);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    [TestMethod]
    public async Task ResolveAsync_UsesSourceAwareAliasForCopiedPlanningLogContext()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var sourcePath = Path.Combine(tempRoot, "Notes", "2026", "April", "4.2.2026", "wct", "planning,self,triage,march-to-april", "log.md");
            var targetPath = Path.Combine(tempRoot, "Notes", "2026", "March", "3.26.2026", "wct", "planning,log.md");
            await CreateTextFileAsync(sourcePath);
            await CreateTextFileAsync(targetPath);

            var resolvedFile = await new RelativePathAssetResolver().ResolveAsync(new SystemFile(sourcePath), "../../planning,log.md");

            Assert.IsNotNull(resolvedFile);
            Assert.AreEqual(targetPath, ((SystemFile)resolvedFile).Path);
        }
        finally
        {
            DeleteTempRoot(tempRoot);
        }
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "wac-blog-resolver-tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private static async Task CreateTextFileAsync(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException("File path has no directory."));
        await File.WriteAllTextAsync(path, string.Empty);
    }

    private static void DeleteTempRoot(string tempRoot)
    {
        if (Directory.Exists(tempRoot))
            Directory.Delete(tempRoot, recursive: true);
    }
}