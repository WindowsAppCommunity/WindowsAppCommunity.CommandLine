using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.PostPage;

namespace WindowsAppCommunity.Blog.Page
{
    /// <summary>
    /// Virtual IFolder representing folderized single-page output structure.
    /// Base class - wraps markdown source file and template to provide virtual {filename}/index.html + assets structure.
    /// Implements lazy generation - no file system operations during construction.
    /// </summary>
    public class HtmlTemplatedMarkdownPageFolder : IChildFolder
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
        public HtmlTemplatedMarkdownPageFolder(IFile markdownSource, IStorable templateSource, string? templateFileName = null)
        {
            _markdownSource = markdownSource ?? throw new ArgumentNullException(nameof(markdownSource));
            _templateSource = templateSource ?? throw new ArgumentNullException(nameof(templateSource));
            _templateFileName = templateFileName;
        }

        /// <summary>
        /// Gets the markdown source file for derived class access.
        /// </summary>
        protected IFile MarkdownSource => _markdownSource;

        /// <summary>
        /// Gets the template source for derived class access.
        /// </summary>
        protected IStorable TemplateSource => _templateSource;

        /// <summary>
        /// Gets the template file name for derived class access.
        /// </summary>
        protected string? TemplateFileName => _templateFileName;

        /// <inheritdoc />
        public string Id => _markdownSource.Id;

        /// <inheritdoc />
        public string Name => SanitizeFilename(_markdownSource.Name);

        /// <summary>
        /// Optional parent folder in virtual hierarchy.
        /// </summary>
        public IFolder? Parent { get; set; }

        /// <inheritdoc />
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Parent);
        }

        /// <inheritdoc />
        public virtual async IAsyncEnumerable<IStorableChild> GetItemsAsync(
            StorableType type = StorableType.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Yield HtmlTemplatedMarkdownFile (virtual HTML file) only
            // Template assets are NOT yielded here - they're detected as links in the template HTML
            // and tracked in the HTML file's IncludedAssets collection for consumer materialization
            if (type == StorableType.All || type == StorableType.File)
            {
                var indexHtmlId = $"{Id}/index.html";
                yield return new HtmlTemplatedMarkdownFile(indexHtmlId, _markdownSource, _templateSource, _templateFileName, this)
                {
                    Name = "index.html"
                };
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
                    throw new FileNotFoundException($"Template file '{fileName}' not found in folder '{folder.Name}'.");
                }

                return resolvedFile;
            }

            throw new ArgumentException($"Template source must be IFile or IFolder, got: {templateSource.GetType().Name}", nameof(templateSource));
        }
    }
}