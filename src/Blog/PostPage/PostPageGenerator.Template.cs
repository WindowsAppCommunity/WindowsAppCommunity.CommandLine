using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OwlCore.Storage;
using Scriban;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Template processing operations for PostPageGenerator.
    /// Handles template resolution, asset copying, and Scriban rendering.
    /// </summary>
    public partial class PostPageGenerator
    {
        /// <summary>
        /// Resolve template file from IStorable source.
        /// Handles both IFile (single template) and IFolder (template + assets).
        /// Uses convention-based lookup ("template.html") when source is folder.
        /// </summary>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">File name when source is IFolder (defaults to "template.html")</param>
        /// <returns>Resolved template IFile</returns>
        private async Task<IFile> ResolveTemplateFileAsync(
            IStorable templateSource,
            string? templateFileName)
        {
            // Gap #8 resolution: Type detection using pattern matching
            if (templateSource is IFile file)
            {
                // Direct file reference
                return file;
            }

            if (templateSource is IFolder folder)
            {
                // Gap #3 resolution: Convention-based template file lookup
                var fileName = templateFileName ?? "template.html";
                var templateFile = await folder.GetFirstByNameAsync(fileName);

                if (templateFile is not IFile resolvedFile)
                {
                    throw new FileNotFoundException(
                        $"Template file '{fileName}' not found in folder '{folder.Name}'.");
                }

                return resolvedFile;
            }

            throw new ArgumentException(
                $"Template source must be IFile or IFolder, got: {templateSource.GetType().Name}",
                nameof(templateSource));
        }

        /// <summary>
        /// Recursively copy all assets from template folder to output folder.
        /// Excludes template file itself to avoid duplication.
        /// Preserves folder structure using OwlCore.Storage recursive operations.
        /// </summary>
        /// <param name="templateFolder">Source template folder</param>
        /// <param name="outputFolder">Destination output folder</param>
        /// <param name="templateFile">Template file to exclude from copy</param>
        private async Task CopyTemplateAssetsAsync(
            IFolder templateFolder,
            IFolder outputFolder,
            IFile templateFile)
        {
            // Gap #11 resolution: Use DepthFirstRecursiveFolder for recursive traversal
            var recursiveFolder = new DepthFirstRecursiveFolder(templateFolder);

            // Gap #7 resolution: Filter files and exclude template file by ID comparison
            await foreach (var item in recursiveFolder.GetItemsAsync(StorableType.File))
            {
                if (item is not IFile file)
                    continue;

                // Exclude the template file itself
                if (file.Id == templateFile.Id)
                    continue;

                // Copy asset to output folder (overwrite silently per Gap #6)
                if (outputFolder is IModifiableFolder modifiableOutput)
                {
                    await modifiableOutput.CreateCopyOfAsync(file, overwrite: true);
                }
            }
        }

        /// <summary>
        /// Render Scriban template with data model to produce final HTML.
        /// Template generates all HTML including meta tags from model.frontmatter.
        /// Flow boundary: Generator provides data model, template generates HTML.
        /// </summary>
        /// <param name="templateFile">Scriban template file</param>
        /// <param name="model">PostPageDataModel with body, frontmatter, metadata</param>
        /// <returns>Rendered HTML string</returns>
        private async Task<string> RenderTemplateAsync(
            IFile templateFile,
            PostPageDataModel model)
        {
            // Read template content
            var templateContent = await templateFile.ReadTextAsync();

            // Parse Scriban template
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
            {
                var errors = string.Join(Environment.NewLine, template.Messages);
                throw new InvalidOperationException($"Template parsing failed:{Environment.NewLine}{errors}");
            }

            // Render template with model
            var html = template.Render(model);

            return html;
        }
    }
}
