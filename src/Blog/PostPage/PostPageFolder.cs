using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Virtual IFolder representing folderized single-page output structure.
    /// Wraps markdown source file and template to provide virtual {filename}/index.html + assets structure.
    /// Implements lazy generation - no file system operations during construction.
    /// </summary>
    public sealed class PostPageFolder : IFolder
    {
        private readonly IFile _markdownSource;
        private readonly IStorable _templateSource;
        private readonly string? _templateFileName;

        /// <summary>
        /// Creates virtual folder representing single-page output structure.
        /// No file system operations occur during construction (lazy generation).
        /// </summary>
        /// <param name="markdownSource">Source markdown file to transform</param>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        public PostPageFolder(IFile markdownSource, IStorable templateSource, string? templateFileName = null)
        {
            _markdownSource = markdownSource ?? throw new ArgumentNullException(nameof(markdownSource));
            _templateSource = templateSource ?? throw new ArgumentNullException(nameof(templateSource));
            _templateFileName = templateFileName;
        }

        /// <inheritdoc />
        public string Id => _markdownSource.Id;

        /// <inheritdoc />
        public string Name => SanitizeFilename(_markdownSource.Name);

        /// <inheritdoc />
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(
            StorableType type = StorableType.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Resolve template file for exclusion and IndexHtmlFile construction
            var templateFile = await ResolveTemplateFileAsync(_templateSource, _templateFileName);

            // Yield IndexHtmlFile (virtual index.html)
            if (type == StorableType.All || type == StorableType.File)
            {
                var indexHtmlId = $"{Id}/index.html";
                yield return new IndexHtmlFile(indexHtmlId, _markdownSource, _templateSource, _templateFileName);
            }

            // If template is folder, yield wrapped asset structure
            if (_templateSource is IFolder templateFolder)
            {
                await foreach (var item in templateFolder.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    // Wrap subfolders as PostPageAssetFolder
                    if (item is IFolder subfolder && (type == StorableType.All || type == StorableType.Folder))
                    {
                        yield return new PostPageAssetFolder(subfolder, this, templateFile);
                        continue;
                    }

                    // Pass through files directly (excluding template HTML file)
                    if (item is IChildFile file && (type == StorableType.All || type == StorableType.File))
                    {
                        // Exclude template HTML file (already rendered as index.html)
                        if (file.Id == templateFile.Id)
                        {
                            continue;
                        }

                        yield return file;
                    }
                }
            }
        }

        /// <summary>
        /// Sanitize markdown filename for use as folder name.
        /// Removes file extension and replaces invalid filename characters with underscore.
        /// </summary>
        /// <param name="markdownFilename">Original markdown filename with extension</param>
        /// <returns>Sanitized folder name</returns>
        private string SanitizeFilename(string markdownFilename)
        {
            // Remove file extension
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(markdownFilename);

            // Replace invalid filename characters with underscore
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Concat(nameWithoutExtension.Select(c => 
                invalidChars.Contains(c) ? '_' : c));

            return sanitized;
        }

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
            if (templateSource is IFile file)
            {
                return file;
            }

            if (templateSource is IFolder folder)
            {
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
    }
}
