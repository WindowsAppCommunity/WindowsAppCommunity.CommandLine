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
                name: "--template-file",
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
            // Resolve markdown source folder (SystemFolder throws if doesn't exist)
            var markdownSourceFolder = new SystemFolder(markdownFolderPath);

            // Resolve template source (file or folder)
            IStorable templateSource;
            if (Directory.Exists(templatePath))
            {
                templateSource = new SystemFolder(templatePath);
            }
            else
            {
                // SystemFile throws if doesn't exist
                templateSource = new SystemFile(templatePath);
            }

            // Resolve output folder (SystemFolder throws if doesn't exist)
            IModifiableFolder outputFolder = new SystemFolder(outputPath);

            // Create virtual AssetAwareHtmlTemplatedMarkdownPagesFolder (lazy generation - no I/O during construction)
            var pagesFolder = new AssetAwareHtmlTemplatedMarkdownPagesFolder(markdownSourceFolder, templateSource, templateFileName)
            {
                LinkDetector = new RegexAssetLinkDetector(),
                Resolver = new RelativePathAssetResolver
                {
                    // MarkdownSource passed per-call in ResolveAsync (varies per page)
                    SourceFolder = markdownSourceFolder
                },
                InclusionStrategy = new ReferenceOnlyInclusionStrategy()
            };

            // Materialize virtual structure by iterating page folders, then files within each
            // Pattern from PostPageCommand: Create output folder per page, copy files relative to it
            await foreach (var item in pagesFolder.GetItemsAsync(StorableType.Folder))
            {
                if (item is not IChildFolder pageFolder)
                    continue;
                
                Logger.LogInformation($"Processing page folder: {pageFolder.Name}");
                
                // Create output folder for this page
                var pageOutputFolder = await outputFolder.CreateFolderAsync(pageFolder.Name, overwrite: true);
                
                // Iterate files within this page folder recursively
                var recursiveFolder = new DepthFirstRecursiveFolder(pageFolder);
                await foreach (var fileItem in recursiveFolder.GetItemsAsync(StorableType.File))
                {
                    if (fileItem is not IChildFile file)
                        continue;
                    
                    Logger.LogInformation($"  Yielded file: {file.Name} (Type: {file.GetType().Name})");
                    
                    // Get relative path from page folder (not pagesFolder root)
                    string relativePath = await pageFolder.GetRelativePathToAsync(file);
                    Logger.LogInformation($"  Relative path: {relativePath}");
                    
                    // Create folders relative to THIS page's output folder
                    var containingFolder = (IModifiableFolder)await pageOutputFolder.CreateFoldersAlongRelativePathAsync(relativePath, overwrite: false).LastAsync();
                    Logger.LogInformation($"  Containing folder: {containingFolder.Id}");
                    
                    // Copy file
                    Logger.LogInformation($"  About to copy - file.Id: {file.Id}, file.GetType(): {file.GetType().Name}");
                    Logger.LogInformation($"  Target folder: {containingFolder.Id}");
                    var copiedFile = await containingFolder.CreateCopyOfAsync(file, overwrite: true);
                    Logger.LogInformation($"  Copied to: {copiedFile.Id}");
                    
                    // Check what else got created
                    Logger.LogInformation($"  Checking folder contents after copy:");
                    await foreach (var folderItem in containingFolder.GetItemsAsync())
                    {
                        Logger.LogInformation($"    Found: {folderItem.Name} (Type: {folderItem.GetType().Name})");
                    }
                    
                    // Copy all assets referenced in content using RewrittenPath
                    if (file is AssetAwareHtmlTemplatedMarkdownFile htmlFile)
                    {
                        foreach (var asset in htmlFile.IncludedAssets)
                        {
                            // Navigate FROM htmlFile using RewrittenPath (relative to HTML file)
                            var assetOutputFolder = (IModifiableFolder)await copiedFile.CreateFoldersAlongRelativePathAsync(asset.RewrittenPath, overwrite: false).LastAsync();
                            await assetOutputFolder.CreateCopyOfAsync(asset.ResolvedFile, overwrite: true);
                        }
                    }
                }
            }

            // Report success
            Logger.LogInformation($"Generated multi-page site: {outputPath}");

            // Return success exit code
            return 0;
        }
    }
}
