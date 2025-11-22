using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.PostPage;

namespace WindowsAppCommunity.Blog.Page
{
    /// <summary>
    /// Asset-aware virtual HTML file - extends base with link detection, asset resolution, and inclusion decisions.
    /// Sealed - this is the final asset-aware implementation.
    /// Implements link rewriting and asset tracking during post-processing.
    /// </summary>
    public sealed class AssetAwareHtmlTemplatedMarkdownFile : HtmlTemplatedMarkdownFile
    {
        private readonly List<ReferencedAsset> _includedAssets = new();
        private readonly IStorable _templateSource;
        private readonly string? _templateFileName;

        /// <summary>
        /// Creates asset-aware virtual HTML file with lazy markdown→HTML generation and asset management.
        /// </summary>
        /// <param name="id">Unique identifier for this file (parent-derived)</param>
        /// <param name="markdownSource">Source markdown file to transform</param>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        /// <param name="parent">Parent folder in virtual hierarchy (optional)</param>
        public AssetAwareHtmlTemplatedMarkdownFile(
            string id, 
            IFile markdownSource, 
            IStorable templateSource, 
            string? templateFileName = null, 
            IFolder? parent = null)
            : base(id, markdownSource, templateSource, templateFileName, parent)
        {
            _templateSource = templateSource;
            _templateFileName = templateFileName;
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
        public required IAssetInclusionStrategy InclusionStrategy { get; init; }

        /// <summary>
        /// All assets referenced by the markdown file (both included and referenced).
        /// Exposed to containing folder for materialization to output.
        /// </summary>
        public IReadOnlyCollection<ReferencedAsset> IncludedAssets => _includedAssets.AsReadOnly();

        /// <summary>
        /// Post-process HTML with asset management pipeline.
        /// Detects links → Resolves to files → Decides include/reference via path rewriting → Tracks included assets.
        /// Detects links from BOTH markdown source AND template file to unify asset handling.
        /// </summary>
        /// <param name="html">Rendered HTML from template</param>
        /// <param name="model">Data model used for rendering</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Post-processed HTML with rewritten links</returns>
        protected override async Task<string> PostProcessHtmlAsync(string html, PostPageDataModel model, CancellationToken ct)
        {
            // Clear included assets from any previous generation
            _includedAssets.Clear();

            // Detect asset links from markdown source (content-referenced assets)
            await foreach (var originalPath in LinkDetector.DetectAsync(MarkdownSource, ct))
            {
                html = await ProcessAssetLinkAsync(html, MarkdownSource, originalPath, ct);
            }

            return html;
        }

        /// <summary>
        /// Process a single detected asset link through the asset pipeline.
        /// Shared logic for both markdown and template asset detection.
        /// </summary>
        /// <param name="html">HTML content to update</param>
        /// <param name="contextFile">File providing resolution context (markdown or template)</param>
        /// <param name="originalPath">Original asset path as detected</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Updated HTML with rewritten link</returns>
        private async Task<string> ProcessAssetLinkAsync(
            string html, 
            IFile contextFile, 
            string originalPath, 
            CancellationToken ct)
        {
            // Resolve path to IFile (pass context file for resolution)
            var resolvedAsset = await Resolver.ResolveAsync(contextFile, originalPath, ct);

            // Null resolver policy: Skip if not found (preserve broken link)
            if (resolvedAsset == null)
            {
                return html;
            }

            // Strategy decides include vs reference by returning rewritten path
            // Path structure determines behavior:
            // - Child path (no ../ prefix): Include
            // - Parent path (../ prefix): Reference
            var rewrittenPath = await InclusionStrategy.DecideAsync(
                contextFile,
                resolvedAsset,
                originalPath,
                ct);

            // Track all referenced assets for materialization
            _includedAssets.Add(new ReferencedAsset(originalPath, rewrittenPath, resolvedAsset));

            // Rewrite link in HTML (strategy determines path prefix)
            return html.Replace(originalPath, rewrittenPath);
        }
    }
}
