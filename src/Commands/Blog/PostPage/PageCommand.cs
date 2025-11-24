using System.CommandLine;
using OwlCore.Extensions;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Page;

namespace WindowsAppCommunity.CommandLine.Blog.PostPage;

/// <summary>
/// CLI command for Post/Page scenario blog generation.
/// Handles command-line parsing and invokes PostPageGenerator.
/// </summary>
public class PageCommand : Command
{
    /// <summary>
    /// Initialize Post/Page command with CLI options.
    /// </summary>
    public PageCommand()
        : base("page", "Generate HTML from markdown using template")
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
    private async Task<int> ExecuteAsync(string markdownPath, string templatePath, string outputPath, string? templateFileName)
    {
        // Gap #5 resolution: SystemFile/SystemFolder constructors validate existence
        // Gap #10 resolution: Directory.Exists distinguishes folders from files
        
        // Resolve markdown file (SystemFile throws if doesn't exist)
        var markdownFile = new SystemFile(markdownPath);

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

        // Create virtual PostPageFolder (lazy generation - no I/O during construction)
        var postPageFolder = new AssetAwareHtmlTemplatedMarkdownPageFolder(markdownFile, templateSource, templateFileName)
        {
            Id = markdownFile.Id.HashMD5Fast(),
            // Single-file page output includes assets by default
            // Unlike multi-page output which references assets by default
            AssetStrategy = new KnownAssetStrategy(),
            Resolver = new RelativePathAssetResolver(),
            LinkDetector = new RegexAssetLinkDetector(),
        };

        // Create output folder for this page
        var pageOutputFolder = await outputFolder.CreateFolderAsync(postPageFolder.Name, overwrite: true);

        // Materialize virtual structure by recursively copying all files
        await foreach (AssetAwareHtmlTemplatedMarkdownFile file in new DepthFirstRecursiveFolder(postPageFolder).GetFilesAsync())
        {
            // TODO, see https://discord.com/channels/372137812037730304/1396673230013464636/1441902694196449505
        }

        var outputFolderName = Path.GetFileNameWithoutExtension(markdownFile.Name);
        Console.WriteLine($"Generated: {Path.Combine(outputPath, outputFolderName, "index.html")}");

        return 0;
    }
}
