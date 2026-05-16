using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.Blog.Pages;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Page;
using OwlCore.Diagnostics;

namespace WindowsAppCommunity.CommandLine.Blog.PostPage
{
    /// <summary>
    /// CLI command for multi-page blog generation (Pages scenario).
    /// Handles command-line parsing and invokes AssetAwareHtmlTemplatedMarkdownPagesFolder.
    /// </summary>
    public class PagesCommand : Command
    {
        /// <summary>
        /// Initialize Pages command with CLI options.
        /// </summary>
        public PagesCommand()
            : base("pages", "Generate multi-page HTML site from markdown folder")
        {
            // Define CLI options
            var markdownFolderOption = new Option<string>(
                name: "--markdown-folder",
                description: "Path to folder containing markdown files to transform")
            {
                IsRequired = true
            };

            var templateOption = new Option<string>(
                name: "--template",
                description: "Path to template file or folder")
            {
                IsRequired = true
            };

            var outputOption = new Option<string>(
                name: "--output",
                description: "Path to output destination folder")
            {
                IsRequired = true
            };

            var templateFileNameOption = new Option<string?>(
                name: "--template-file-name",
                description: "Template file name when --template is folder (optional, defaults to 'template.html')",
                getDefaultValue: () => null);

            // Register options
            AddOption(markdownFolderOption);
            AddOption(templateOption);
            AddOption(outputOption);
            AddOption(templateFileNameOption);

            // Set handler with option parameters
            this.SetHandler(ExecuteAsync, markdownFolderOption, templateOption, outputOption, templateFileNameOption);
        }

        /// <summary>
        /// Execute multi-page generation command.
        /// Orchestrates: Parse arguments → Resolve storage → Invoke generator → Report results
        /// </summary>
        /// <param name="markdownFolderPath">Path to folder containing markdown files</param>
        /// <param name="templatePath">Path to template file or folder</param>
        /// <param name="outputPath">Path to output destination folder</param>
        /// <param name="templateFileName">Template file name when template is folder (optional)</param>
        /// <returns>Exit code (0 = success, non-zero = error)</returns>
        private async Task<int> ExecuteAsync(
            string markdownFolderPath,
            string templatePath,
            string outputPath,
            string? templateFileName)
        {
            // Resolve template source (file or folder)
            IStorable templateSource = Directory.Exists(templatePath)
                ? new SystemFolder(templatePath)
                : new SystemFile(templatePath);

            // Resolve markdown source and output folders (SystemFolder throws if doesn't exist)
            var outputFolder = new SystemFolder(outputPath);
            var markdownSourceFolder = new SystemFolder(markdownFolderPath);

            // Create recursive markdown-to-webpage folder (lazy generation - no I/O during construction)
            // Turns `.md` files into folders with an `index.html` holding asset metadata for output copy
            var templateFileIds = await PageAssetMaterializer.GetFileIdsAsync(templateSource);
            var markdownSourceFileIds = await PageAssetMaterializer.GetFileIdsAsync(markdownSourceFolder);
            var markdownPageRouteIndex = await MarkdownPageRouteIndex.CreateAsync(markdownSourceFolder);
            var fileAssetStrategy = new KnownAssetStrategy()
            {
                IncludedAssetFileIds = templateFileIds,
                ReferencedAssetFileIds = markdownSourceFileIds,
                UnknownAssetFaultStrategy = FaultStrategy.LogWarn,
                UnknownAssetFallbackStrategy = AssetFallbackBehavior.Drop,
            };

            var pagesFolder = new AssetAwareHtmlTemplatedMarkdownPagesFolder(markdownSourceFolder, templateSource, templateFileName)
            {
                LinkDetector = new RegexAssetLinkDetector(),
                Resolver = new RelativePathAssetResolver(),
                AssetStrategy = new MarkdownPageAssetStrategy
                {
                    RouteIndex = markdownPageRouteIndex,
                    AssetStrategy = fileAssetStrategy,
                },
            };

            // Materialize virtual folderized markdown pages, then files within each markdown page folder.
            await foreach (IChildFolder pageFolder in new DepthFirstRecursiveFolder(pagesFolder).GetItemsAsync(StorableType.Folder))
            {
                // Get path to markdown page folder (mirrors original source file without extension)
                var relativePathToPagesPageFolder = await pagesFolder.GetRelativePathToAsync(pageFolder);
                var pageOutputFolder = (IModifiableFolder)await outputFolder.CreateFoldersAlongRelativePathAsync(relativePathToPagesPageFolder, overwrite: false).LastAsync();

                // Iterate/copy files within markdown page folder
                await foreach (AssetAwareHtmlTemplatedMarkdownFile indexFile in pageFolder.GetItemsAsync(StorableType.File))
                {
                    // Create folders relative to THIS page's output folder, then copy
                    await pageOutputFolder.CreateCopyOfAsync(indexFile, overwrite: true);
                    await PageAssetMaterializer.CopyAssetsAsync(pageOutputFolder, indexFile.Assets);
                }
            }

            // Report success
            Logger.LogInformation($"Generated multi-page site: {outputPath}");

            // Return success exit code
            return 0;
        }
    }
}
