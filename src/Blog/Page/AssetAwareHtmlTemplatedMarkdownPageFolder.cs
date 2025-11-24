using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;

namespace WindowsAppCommunity.Blog.Page
{
    /// <summary>
    /// Asset-aware virtual folder - extends base with markdown-referenced asset inclusion.
    /// Creates asset-aware file variant and yields included assets in virtual structure.
    /// Implements lazy generation - no file system operations during construction.
    /// </summary>
    public class AssetAwareHtmlTemplatedMarkdownPageFolder : HtmlTemplatedMarkdownPageFolder
    {
        /// <summary>
        /// Creates asset-aware virtual folder representing single-page output structure with asset management.
        /// No file system operations occur during construction (lazy generation).
        /// </summary>
        /// <param name="markdownSource">Source markdown file to transform</param>
        /// <param name="templateSource">Template as IFile or IFolder</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        public AssetAwareHtmlTemplatedMarkdownPageFolder(IFile markdownSource, IStorable templateSource, string? templateFileName = null)
            : base(markdownSource, templateSource, templateFileName)
        {
        }

        /// <summary>
        /// Asset link detector for finding relative links in markdown.
        /// </summary>
        public required IAssetLinkDetector LinkDetector { get; init; }

        /// <summary>
        /// Asset resolver for converting paths to IFile instances.
        /// </summary>
        public required IAssetResolver Resolver { get; init; }

        /// <summary>
        /// Inclusion strategy for deciding include vs reference per asset.
        /// </summary>
        public required IAssetStrategy AssetStrategy { get; init; }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Yield base items (HTML file + template assets), capturing asset-aware file reference
            await foreach (var item in base.GetItemsAsync(type, cancellationToken))
            {
                // Intercept HTML file creation to replace with asset-aware variant
                if (item is HtmlTemplatedMarkdownFile htmlFile)
                {
                    // Create asset-aware variant with required properties set
                    yield return new AssetAwareHtmlTemplatedMarkdownFile(htmlFile.Id, MarkdownSource, TemplateSource, TemplateFileName, this)
                    {
                        Name = htmlFile.Name,
                        Created = htmlFile.Created,
                        Modified = htmlFile.Modified,
                        LinkDetector = LinkDetector,
                        Resolver = Resolver,
                        AssetStrategy = AssetStrategy
                    };
                }
            }
        }
    }
}