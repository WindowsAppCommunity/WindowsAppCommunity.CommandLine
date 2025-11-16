using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using WindowsAppCommunity.Blog.Assets;
using WindowsAppCommunity.Blog.Page;

namespace WindowsAppCommunity.Blog.Pages
{
    /// <summary>
    /// Multi-page composition root - discovers markdown files and preserves folder hierarchy through virtual structure nesting.
    /// Asset-aware only variant (no non-asset-aware needed for multi-page scenario).
    /// Implements lazy generation - no file system operations during construction.
    /// </summary>
    public class AssetAwareHtmlTemplatedMarkdownPagesFolder : IFolder
    {
        private readonly IFolder _markdownSourceFolder;
        private readonly IStorable _templateSource;
        private readonly string? _templateFileName;

        /// <summary>
        /// Creates multi-page composition root with recursive structure preservation and asset management.
        /// No file system operations occur during construction (lazy generation).
        /// </summary>
        /// <param name="markdownSourceFolder">Source folder containing markdown files and subfolders (recursive)</param>
        /// <param name="templateSource">Template as IFile or IFolder (shared across all pages)</param>
        /// <param name="templateFileName">Template file name when source is IFolder (defaults to "template.html")</param>
        public AssetAwareHtmlTemplatedMarkdownPagesFolder(
            IFolder markdownSourceFolder,
            IStorable templateSource,
            string? templateFileName = null)
        {
            _markdownSourceFolder = markdownSourceFolder ?? throw new ArgumentNullException(nameof(markdownSourceFolder));
            _templateSource = templateSource ?? throw new ArgumentNullException(nameof(templateSource));
            _templateFileName = templateFileName;
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
        public required IAssetInclusionStrategy InclusionStrategy { get; init; }

        /// <inheritdoc />
        public string Id => _markdownSourceFolder.Id;

        /// <inheritdoc />
        public string Name => _markdownSourceFolder.Name;

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
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Enumerate source folder items
            await foreach (var item in _markdownSourceFolder.GetItemsAsync(StorableType.All, cancellationToken))
            {
                // Markdown files → create asset-aware page folders
                if (item is IFile file && Path.GetExtension(file.Name).Equals(".md", StringComparison.OrdinalIgnoreCase))
                {
                    if (type == StorableType.All || type == StorableType.Folder)
                    {
                        var pageFolder = new AssetAwareHtmlTemplatedMarkdownPageFolder(
                            file,
                            _templateSource,
                            _templateFileName)
                        {
                            LinkDetector = LinkDetector,
                            Resolver = Resolver,
                            InclusionStrategy = InclusionStrategy,
                            Parent = this
                        };

                        yield return (IStorableChild)pageFolder;
                    }
                }

                // Subfolders → create nested pages folders (recursive preservation)
                if (item is IFolder subfolder)
                {
                    if (type == StorableType.All || type == StorableType.Folder)
                    {
                        var nestedPagesFolder = new AssetAwareHtmlTemplatedMarkdownPagesFolder(
                            subfolder,
                            _templateSource,
                            _templateFileName)
                        {
                            LinkDetector = LinkDetector,
                            Resolver = Resolver,
                            InclusionStrategy = InclusionStrategy,
                            Parent = this
                        };

                        yield return (IStorableChild)nestedPagesFolder;
                    }
                }
            }
        }
    }
}
