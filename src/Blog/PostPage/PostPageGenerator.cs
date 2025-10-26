using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OwlCore.Storage;
using Markdig;
using Scriban;
using YamlDotNet.Serialization;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Transformation orchestrator for Post/Page scenario.
    /// Orchestrates 6 implementation features + data model creation.
    /// Flow: Markdown → Generator → Model → Template → HTML
    /// Generator creates data model, template generates all HTML (including meta tags).
    /// </summary>
    public partial class PostPageGenerator
    {
        /// <summary>
        /// Generate HTML output from markdown source using Scriban template.
        /// Single entry point orchestrating complete transformation flow.
        /// Flow: Markdown → Model → Template → HTML
        /// </summary>
        /// <param name="markdownFile">Source markdown file with optional YAML front-matter</param>
        /// <param name="templateSource">Template file (IFile) or folder (IFolder) containing template</param>
        /// <param name="destinationFolder">Output folder where [name]/index.html will be created</param>
        /// <param name="templateFileName">Template file name when templateSource is IFolder (optional, defaults to "template.html")</param>
        public async Task GenerateAsync(
            IFile markdownFile,
            IStorable templateSource,
            IFolder destinationFolder,
            string? templateFileName = null)
        {
            // 1. Parse markdown (front-matter + content)
            var (frontmatter, content) = await ParseMarkdownAsync(markdownFile);

            // 2. Transform markdown to HTML body
            var body = TransformMarkdownToHtml(content);

            // 3. Parse front-matter YAML
            var frontmatterDict = ParseFrontmatter(frontmatter);

            // 4. Create data model
            var model = CreateDataModel(body, frontmatterDict, markdownFile);

            // 5. Resolve template file
            var templateFile = await ResolveTemplateFileAsync(templateSource, templateFileName);

            // 6. Copy template assets (if template source is folder)
            if (templateSource is IFolder templateFolder)
            {
                // Create output folder first (needed for asset copying)
                var outputFolderName = Path.GetFileNameWithoutExtension(markdownFile.Name);
                var outputFolder = await CreateOutputFolderAsync(destinationFolder, outputFolderName);

                await CopyTemplateAssetsAsync(templateFolder, outputFolder, templateFile);

                // 7. Render template with model
                var html = await RenderTemplateAsync(templateFile, model);

                // 8. Write index.html to output folder
                await WriteIndexHtmlAsync(outputFolder, html);
            }
            else
            {
                // Template is single file (no assets to copy)
                // Create output folder
                var outputFolderName = Path.GetFileNameWithoutExtension(markdownFile.Name);
                var outputFolder = await CreateOutputFolderAsync(destinationFolder, outputFolderName);

                // 7. Render template with model
                var html = await RenderTemplateAsync(templateFile, model);

                // 8. Write index.html
                await WriteIndexHtmlAsync(outputFolder, html);
            }
        }

        /// <summary>
        /// Create PostPageDataModel from transformed content and metadata.
        /// Populates model with body, frontmatter (as-is), and optional metadata.
        /// Flow: Markdown → Model → Template → HTML
        /// Generator provides data, template generates HTML (including meta tags).
        /// </summary>
        /// <param name="body">Transformed HTML body from markdown</param>
        /// <param name="frontmatter">Parsed front-matter dictionary (all keys included)</param>
        /// <param name="sourceFile">Source markdown file for metadata extraction</param>
        /// <returns>Complete data model for template rendering</returns>
        private PostPageDataModel CreateDataModel(
            string body,
            Dictionary<string, object> frontmatter,
            IFile sourceFile)
        {
            return new PostPageDataModel
            {
                Body = body,
                Frontmatter = frontmatter,
                // Note: Timestamps and other metadata can be added here if IFile provides properties
                // For now, template can extract filename and other data from frontmatter
            };
        }

        /// <summary>
        /// Create folderized output structure: [name]/index.html
        /// Name derived from markdown filename, sanitized for filesystem safety.
        /// </summary>
        /// <param name="destination">Parent destination folder</param>
        /// <param name="folderName">Name for output subfolder</param>
        /// <returns>Created output folder</returns>
        private async Task<IFolder> CreateOutputFolderAsync(
            IFolder destination,
            string folderName)
        {
            // Gap #9 resolution: Sanitize folder name
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitizedName = string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

            if (string.IsNullOrWhiteSpace(sanitizedName))
            {
                throw new ArgumentException("Folder name resulted in empty string after sanitization.", nameof(folderName));
            }

            // Gap #6 resolution: Overwrite silently
            if (destination is IModifiableFolder modifiableDestination)
            {
                return await modifiableDestination.CreateFolderAsync(sanitizedName, overwrite: true);
            }

            throw new ArgumentException(
                $"Destination folder must be IModifiableFolder, got: {destination.GetType().Name}",
                nameof(destination));
        }

        /// <summary>
        /// Write rendered HTML to index.html in output folder.
        /// Overwrites existing index.html if present.
        /// </summary>
        /// <param name="outputFolder">Output folder for index.html</param>
        /// <param name="html">Rendered HTML content</param>
        private async Task WriteIndexHtmlAsync(IFolder outputFolder, string html)
        {
            if (outputFolder is not IModifiableFolder modifiableFolder)
            {
                throw new ArgumentException(
                    $"Output folder must be IModifiableFolder, got: {outputFolder.GetType().Name}",
                    nameof(outputFolder));
            }

            // Create or overwrite index.html
            var indexFile = await modifiableFolder.CreateFileAsync("index.html", overwrite: true);

            // Write HTML content using OwlCore.Storage extension
            await indexFile.WriteTextAsync(html);
        }
    }
}
