using OwlCore.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Page;

namespace WindowsAppCommunity.Blog.Page
{
    /// <summary>
    /// Asset-aware virtual HTML file - extends base with link detection, asset resolution, and inclusion decisions.
    /// Sealed - this is the final asset-aware implementation.
    /// Implements link rewriting and asset tracking during post-processing.
    /// </summary>
    public sealed class AssetAwareHtmlTemplatedMarkdownFile : HtmlTemplatedMarkdownFile
    {
        private readonly List<PageAsset> _assets = new();

        /// <summary>
        /// Creates asset-aware virtual HTML file with lazy markdown→HTML generation and asset management.
        /// </summary>
        /// <param name="id">Unique identifier for this file (parent-derived)</param>
        /// <param name="markdownSource">Source markdown file to transform</param>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        /// <param name="parent">Parent folder in virtual hierarchy (optional)</param>
        public AssetAwareHtmlTemplatedMarkdownFile(string id, IFile markdownSource, IStorable templateSource, string? templateFileName = null, IFolder? parent = null)
            : base(id, markdownSource, templateSource, templateFileName, parent)
        {
        }

        /// <summary>
        /// Asset link detector for finding relative links in rendered HTML output.
        /// </summary>
        public required IAssetLinkDetector LinkDetector { get; init; }

        /// <summary>
        /// Asset resolver for converting paths to IFile instances.
        /// </summary>
        public required IAssetResolver Resolver { get; init; }

        /// <summary>
        /// Inclusion strategy for deciding include vs reference via path rewriting.
        /// </summary>
        public required IAssetStrategy AssetStrategy { get; init; }

        /// <summary>
        /// All assets referenced by the markdown file (both included and referenced).
        /// Exposed to containing folder for materialization to output.
        /// </summary>
        public IReadOnlyCollection<PageAsset> Assets => _assets;

        /// <summary>
        /// Post-process HTML with asset management pipeline.
        /// Detects links → Resolves to files → Decides include/reference via path rewriting → Tracks included assets.
        /// Detects links from BOTH markdown source AND template file to unify asset handling.
        /// </summary>
        /// <param name="templateFile">The resolved HTML template file.</param>
        /// <param name="model">Data model used for rendering</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Post-processed HTML with rewritten links</returns>
        protected override async Task<string> RenderTemplateAsync(IFile templateFile, HtmlMarkdownDataTemplateModel model, CancellationToken cancellationToken)
        {
            // Clear included assets from any previous generation
            _assets.Clear();

            await foreach (var originalPath in LinkDetector.DetectAsync(templateFile, cancellationToken))
            {
                var referencedAsset = await ProcessAssetLinkAsync(templateFile, originalPath, cancellationToken);
                if (referencedAsset is null)
                    continue;

                _assets.Add(referencedAsset);
            }

            var html = await base.RenderTemplateAsync(templateFile, model, cancellationToken);

            // Detect asset links from markdown source (content-referenced assets)
            await foreach (var originalPath in LinkDetector.DetectAsync(MarkdownSource, cancellationToken))
            {
                var referencedAsset = await ProcessAssetLinkAsync(MarkdownSource, originalPath, cancellationToken);
                if (referencedAsset is null)
                    continue;

                _assets.Add(referencedAsset);
                html = html.Replace(referencedAsset.OriginalPath, referencedAsset.RewrittenPath);
            }

            return html;
        }

        /// <summary>
        /// Process a single detected asset link through the asset pipeline.
        /// Shared logic for both markdown and template asset detection.
        /// </summary>
        /// <param name="contextFile">File providing resolution context (markdown or template)</param>
        /// <param name="originalPath">Original asset path as detected</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Updated HTML with rewritten link</returns>
        private async Task<PageAsset?> ProcessAssetLinkAsync(IFile contextFile, string originalPath, CancellationToken cancellationToken)
        {
            // Resolve path to IFile (pass context file for resolution)
            var resolvedAsset = await Resolver.ResolveAsync(contextFile, originalPath, cancellationToken);

            // Skip if not found
            if (resolvedAsset == null)
                return null;

            // Strategy decides include vs reference by returning rewritten path
            // Path structure determines behavior:
            // - Child path (no ../ prefix): Include
            // - Parent path (../ prefix): Reference
            var rewrittenPath = await AssetStrategy.DecideAsync(contextFile, resolvedAsset, originalPath, cancellationToken);
            if (rewrittenPath is null)
                return null;

            // Track all referenced assets for materialization
            return new PageAsset(originalPath, rewrittenPath, resolvedAsset);
        }
    }
}
