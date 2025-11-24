using System.Text;
using Markdig;
using OwlCore.Storage;
using Scriban;
using YamlDotNet.Serialization;
using WindowsAppCommunity.Blog.Page;

namespace WindowsAppCommunity.Blog.Page
{
    /// <summary>
    /// Virtual IChildFile representing HTML generated from markdown source with template.
    /// Base class - provides core markdown→HTML transformation pipeline with extensibility hooks.
    /// Implements lazy generation - markdown→HTML transformation occurs on OpenStreamAsync.
    /// Read-only - throws NotSupportedException for write operations.
    /// </summary>
    public class HtmlTemplatedMarkdownFile : IChildFile
    {
        private readonly string _id;
        private readonly IFile _markdownSource;
        private readonly IStorable _templateSource;
        private readonly string? _templateFileName;
        private readonly IFolder? _parent;

        /// <summary>
        /// Creates virtual HTML file with lazy markdown→HTML generation.
        /// </summary>
        /// <param name="id">Unique identifier for this file (parent-derived)</param>
        /// <param name="markdownSource">Source markdown file to transform</param>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        /// <param name="parent">Parent folder in virtual hierarchy (optional)</param>
        public HtmlTemplatedMarkdownFile(string id, IFile markdownSource, IStorable templateSource, string? templateFileName = null, IFolder? parent = null)
        {
            _id = id ?? throw new ArgumentNullException(nameof(id));
            _markdownSource = markdownSource ?? throw new ArgumentNullException(nameof(markdownSource));
            _templateSource = templateSource ?? throw new ArgumentNullException(nameof(templateSource));
            _templateFileName = templateFileName;
            _parent = parent;
        }

        /// <inheritdoc />
        public string Id => _id;

        /// <inheritdoc />
        /// <remarks>
        /// Required property - consumer must set via object initializer.
        /// No default value provided (e.g., "index.html" is not assumed).
        /// </remarks>
        public required string Name { get; init; }

        /// <summary>
        /// File creation timestamp from filesystem metadata.
        /// </summary>
        public DateTime? Created { get; set; }

        /// <summary>
        /// File modification timestamp from filesystem metadata.
        /// </summary>
        public DateTime? Modified { get; set; }

        /// <summary>
        /// Source markdown file being transformed.
        /// Exposed for derived class access (e.g., passing to asset strategies).
        /// </summary>
        public IFile MarkdownSource => _markdownSource;

        /// <inheritdoc />
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_parent);
        }

        /// <inheritdoc />
        public async Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            // Read-only file - reject write operations
            if (accessMode == FileAccess.Write || accessMode == FileAccess.ReadWrite)
            {
                throw new NotSupportedException($"{GetType().Name} is read-only. Cannot open with access mode: {accessMode}");
            }

            // Lazy generation: Transform markdown→HTML on every call (no caching)
            var html = await GenerateHtmlAsync(cancellationToken);

            // Convert HTML string to UTF-8 byte stream
            var bytes = Encoding.UTF8.GetBytes(html);
            var stream = new MemoryStream(bytes);
            stream.Position = 0;

            return stream;
        }

        /// <summary>
        /// Generate HTML by transforming markdown source with template.
        /// Orchestrates: Parse markdown → Transform to HTML → Render template → Post-process.
        /// </summary>
        private async Task<string> GenerateHtmlAsync(CancellationToken cancellationToken)
        {
            // Parse markdown file (extract front-matter + content)
            var (frontmatter, content) = await ParseMarkdownAsync(_markdownSource);

            // Transform markdown content to HTML body
            var htmlBody = TransformMarkdownToHtml(content);

            // Parse front-matter YAML to dictionary
            var frontmatterDict = ParseFrontmatter(frontmatter);

            // Resolve template file from IStorable source
            var templateFile = await ResolveTemplateFileAsync(_templateSource, _templateFileName);

            // Create data model for template
            var model = new HtmlMarkdownDataTemplateModel
            {
                Body = htmlBody,
                Frontmatter = frontmatterDict,
                Filename = _markdownSource.Name,
                Created = Created,
                Modified = Modified
            };

            // Render template with model
            return await RenderTemplateAsync(templateFile, model, cancellationToken);
        }

        /// <summary>
        /// Extract YAML front-matter block from markdown file.
        /// Front-matter is delimited by "---" at start and end.
        /// Handles files without front-matter (returns empty string for frontmatter).
        /// </summary>
        /// <param name="file">Markdown file to parse</param>
        /// <returns>Tuple of (frontmatter YAML string, content markdown string)</returns>
        protected virtual async Task<(string frontmatter, string content)> ParseMarkdownAsync(IFile file)
        {
            var text = await file.ReadTextAsync();

            // Check for front-matter delimiters
            if (!text.StartsWith("---"))
            {
                // No front-matter present
                return (string.Empty, text);
            }

            // Find the closing delimiter
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            var closingDelimiterIndex = -1;

            for (int i = 1; i < lines.Length; i++)
            {
                if (lines[i].Trim() == "---")
                {
                    closingDelimiterIndex = i;
                    break;
                }
            }

            if (closingDelimiterIndex == -1)
            {
                // No closing delimiter found - treat entire file as content
                return (string.Empty, text);
            }

            // Extract front-matter (lines between delimiters)
            var frontmatterLines = lines.Skip(1).Take(closingDelimiterIndex - 1);
            var frontmatter = string.Join(Environment.NewLine, frontmatterLines);

            // Extract content (everything after closing delimiter)
            var contentLines = lines.Skip(closingDelimiterIndex + 1);
            var content = string.Join(Environment.NewLine, contentLines);

            return (frontmatter, content);
        }

        /// <summary>
        /// Transform markdown content to HTML body using Markdig.
        /// Returns HTML without wrapping elements - template controls structure.
        /// Uses Advanced Extensions pipeline for full Markdown feature support.
        /// </summary>
        /// <param name="markdown">Markdown content string</param>
        /// <returns>HTML body content</returns>
        protected virtual string TransformMarkdownToHtml(string markdown)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseSoftlineBreakAsHardlineBreak()
                .Build();

            return Markdown.ToHtml(markdown, pipeline);
        }

        /// <summary>
        /// Parse YAML front-matter string to arbitrary dictionary.
        /// No schema enforcement - accepts any valid YAML structure.
        /// Handles empty/missing front-matter gracefully.
        /// </summary>
        /// <param name="yaml">YAML string from front-matter</param>
        /// <returns>Dictionary with arbitrary keys and values</returns>
        protected virtual Dictionary<string, object> ParseFrontmatter(string yaml)
        {
            // Handle empty front-matter
            if (string.IsNullOrWhiteSpace(yaml))
            {
                return new Dictionary<string, object>();
            }

            try
            {
                var deserializer = new DeserializerBuilder()
                    .Build();

                var result = deserializer.Deserialize<Dictionary<string, object>>(yaml);
                return result ?? new Dictionary<string, object>();
            }
            catch (YamlDotNet.Core.YamlException ex)
            {
                throw new InvalidOperationException($"Failed to parse YAML front-matter: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Resolve template file from IStorable source.
        /// Handles both IFile (single template) and IFolder (template + assets).
        /// Uses convention-based lookup ("template.html") when source is folder.
        /// </summary>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">File name when source is IFolder (defaults to "template.html")</param>
        /// <returns>Resolved template IFile</returns>
        protected virtual async Task<IFile> ResolveTemplateFileAsync(IStorable templateSource, string? templateFileName)
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

        /// <summary>
        /// Render Scriban template with data model to produce final HTML.
        /// Template generates all HTML including meta tags from model.frontmatter.
        /// Flow boundary: Generator provides data model, template generates HTML.
        /// </summary>
        /// <param name="templateFile">Scriban template file</param>
        /// <param name="model">PostPageDataModel with body, frontmatter, metadata</param>
        /// <param name="cancellationToken">A token that can be used to cancel the ongoing operation.</param>
        /// <returns>Rendered HTML string</returns>
        protected virtual async Task<string> RenderTemplateAsync(IFile templateFile, HtmlMarkdownDataTemplateModel model, CancellationToken cancellationToken)
        {
            var templateContent = await templateFile.ReadTextAsync();
            var template = Template.Parse(templateContent);

            if (template.HasErrors)
            {
                var errors = string.Join(Environment.NewLine, template.Messages);
                throw new InvalidOperationException($"Template parsing failed:{Environment.NewLine}{errors}");
            }

            return template.Render(model);
        }
    }
}