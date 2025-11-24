using System.Runtime.CompilerServices;
using OwlCore.Extensions;
using OwlCore.Storage;

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
        public required string Id { get; init; }

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
        public virtual async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Yield HtmlTemplatedMarkdownFile (virtual HTML file)
            if (type == StorableType.All || type == StorableType.File)
            {
                var indexHtmlId = $"{$"{Id}-index.html".HashMD5Fast()}";
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
    }
}