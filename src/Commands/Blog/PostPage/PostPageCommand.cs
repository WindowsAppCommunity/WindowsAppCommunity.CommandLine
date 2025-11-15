using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Threading.Tasks;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.Blog.PostPage;

namespace WindowsAppCommunity.CommandLine.Blog.PostPage
{
    /// <summary>
    /// CLI command for Post/Page scenario blog generation.
    /// Handles command-line parsing and invokes PostPageGenerator.
    /// </summary>
    public class PostPageCommand : Command
    {
        /// <summary>
        /// Initialize Post/Page command with CLI options.
        /// </summary>
        public PostPageCommand()
            : base("postpage", "Generate HTML from markdown using template")
        {
            // Define CLI options
            var markdownOption = new Option<string>(
                name: "--markdown",
                description: "Path to markdown file to transform")
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
            AddOption(markdownOption);
            AddOption(templateOption);
            AddOption(outputOption);
            AddOption(templateFileNameOption);

            // Set handler with option parameters
            this.SetHandler(ExecuteAsync, markdownOption, templateOption, outputOption, templateFileNameOption);
        }

        /// <summary>
        /// Execute Post/Page generation command.
        /// Orchestrates: Parse arguments → Resolve storage → Invoke generator → Report results
        /// </summary>
        /// <param name="markdownPath">Path to markdown file</param>
        /// <param name="templatePath">Path to template file or folder</param>
        /// <param name="outputPath">Path to output destination folder</param>
        /// <param name="templateFileName">Template file name when template is folder (optional)</param>
        /// <returns>Exit code (0 = success, non-zero = error)</returns>
        private async Task<int> ExecuteAsync(
            string markdownPath,
            string templatePath,
            string outputPath,
            string? templateFileName)
        {
            // Gap #5 resolution: SystemFile/SystemFolder constructors validate existence
            // Gap #10 resolution: Directory.Exists distinguishes folders from files
            
            // 1. Resolve markdown file (SystemFile throws if doesn't exist)
            var markdownFile = new SystemFile(markdownPath);

            // 2. Resolve template source (file or folder)
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

            // 3. Resolve output folder (SystemFolder throws if doesn't exist)
            IModifiableFolder outputFolder = new SystemFolder(outputPath);

            // 4. Create virtual PostPageFolder (lazy generation - no I/O during construction)
            var postPageFolder = new PostPageFolder(markdownFile, templateSource, templateFileName);

            // 5. Create output folder for this page
            var pageOutputFolder = await outputFolder.CreateFolderAsync(postPageFolder.Name, overwrite: true);

            // 6. Materialize virtual structure by recursively copying all files
            var recursiveFolder = new DepthFirstRecursiveFolder(postPageFolder);
            await foreach (var item in recursiveFolder.GetItemsAsync(StorableType.File))
            {
                if (item is not IChildFile file)
                    continue;

                // Get relative path from appropriate root based on file type
                string relativePath;
                if (file is IndexHtmlFile)
                {
                    // IndexHtmlFile is virtual, use simple name-based path
                    relativePath = $"/{file.Name}";
                }
                else if (templateSource is IFolder templateFolder)
                {
                    // Asset files from template folder - get path relative to template root
                    relativePath = await templateFolder.GetRelativePathToAsync(file);
                }
                else
                {
                    // Template is file, no assets exist - skip
                    continue;
                }
                
                // Create containing folder for this file (or open if exists)
                var containingFolder = await pageOutputFolder.CreateFoldersAlongRelativePathAsync(relativePath, overwrite: false).LastAsync();

                // Copy file using ICreateCopyOf fastpath
                await ((IModifiableFolder)containingFolder).CreateCopyOfAsync(file, overwrite: true);
            }

            // 7. Report success
            var outputFolderName = Path.GetFileNameWithoutExtension(markdownFile.Name);
            Console.WriteLine($"Generated: {Path.Combine(outputPath, outputFolderName, "index.html")}");

            // 7. Return success exit code
            return 0;
        }
    }
}
