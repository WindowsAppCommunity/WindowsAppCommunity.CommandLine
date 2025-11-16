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
        private readonly List<IFile> _includedAssets = new();

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
        /// Assets that were decided for inclusion (self-contained in page folder).
        /// Exposed to containing folder for yielding in virtual structure.
        /// </summary>
        public IReadOnlyCollection<IFile> IncludedAssets => _includedAssets.AsReadOnly();

        /// <summary>
        /// Post-process HTML with asset management pipeline.
        /// Detects links → Resolves to files → Decides include/reference via path rewriting → Tracks included assets.
        /// </summary>
        /// <param name="html">Rendered HTML from template</param>
        /// <param name="model">Data model used for rendering</param>
        /// <param name="ct">Cancellation token</param>
        /// <returns>Post-processed HTML with rewritten links</returns>
        protected override async Task<string> PostProcessHtmlAsync(string html, PostPageDataModel model, CancellationToken ct)
        {
            // Clear included assets from any previous generation
            _includedAssets.Clear();

            // Detect asset links in rendered HTML output (pass self as IFile)
            await foreach (var originalPath in LinkDetector.DetectAsync(this, ct))
            {
                // Resolve path to IFile
                var resolvedAsset = await Resolver.ResolveAsync(originalPath, ct);

                // Null resolver policy: Skip if not found (preserve broken link)
                if (resolvedAsset == null)
                {
                    continue;
                }

                // Strategy decides include vs reference by returning rewritten path
                // Path structure determines behavior:
                // - Child path (no ../ prefix): Include
                // - Parent path (../ prefix): Reference
                var rewrittenPath = await InclusionStrategy.DecideAsync(
                    MarkdownSource,  // Pass original markdown source (not virtual HTML)
                    resolvedAsset,
                    originalPath,
                    ct);

                // Implicit decision based on path structure
                if (!rewrittenPath.StartsWith("../"))
                {
                    // Include: Add to tracked assets (will be yielded by containing folder)
                    _includedAssets.Add(resolvedAsset);
                }
                // Reference: Asset not added to included list (stays external)

                // Rewrite link in HTML (applies to both Include and Reference)
                html = html.Replace(originalPath, rewrittenPath);
            }

            return html;
        }
    }
}
