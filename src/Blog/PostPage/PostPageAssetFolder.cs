using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using OwlCore.Storage;

namespace WindowsAppCommunity.Blog.PostPage
{
    /// <summary>
    /// Virtual IChildFolder that recursively wraps template asset folders.
    /// Mirrors template folder structure with recursive PostPageAssetFolder wrapping.
    /// Passes through files directly (preserves type identity for fastpath extension methods).
    /// Propagates template file exclusion down hierarchy.
    /// </summary>
    public sealed class PostPageAssetFolder : IChildFolder
    {
        private readonly IFolder _wrappedFolder;
        private readonly IFolder _parent;
        private readonly IFile? _templateFileToExclude;

        /// <summary>
        /// Creates virtual asset folder wrapping template folder structure.
        /// </summary>
        /// <param name="wrappedFolder">Template folder to mirror</param>
        /// <param name="parent">Parent folder in virtual hierarchy</param>
        /// <param name="templateFileToExclude">Template HTML file to exclude from enumeration</param>
        public PostPageAssetFolder(IFolder wrappedFolder, IFolder parent, IFile? templateFileToExclude)
        {
            _wrappedFolder = wrappedFolder ?? throw new ArgumentNullException(nameof(wrappedFolder));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _templateFileToExclude = templateFileToExclude;
        }

        /// <inheritdoc />
        public string Id => _wrappedFolder.Id;

        /// <inheritdoc />
        public string Name => _wrappedFolder.Name;

        /// <summary>
        /// Parent folder in virtual hierarchy (not interface requirement, internal storage).
        /// </summary>
        public IFolder Parent => _parent;

        /// <inheritdoc />
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(_parent);
        }

        /// <inheritdoc />
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(
            StorableType type = StorableType.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Enumerate wrapped folder items
            await foreach (var item in _wrappedFolder.GetItemsAsync(type, cancellationToken))
            {
                // Recursively wrap subfolders with this as parent
                if (item is IFolder subfolder && (type == StorableType.All || type == StorableType.Folder))
                {
                    yield return new PostPageAssetFolder(subfolder, this, _templateFileToExclude);
                    continue;
                }

                // Pass through files directly (preserves type identity)
                if (item is IChildFile file && (type == StorableType.All || type == StorableType.File))
                {
                    // Exclude template HTML file if specified
                    if (_templateFileToExclude != null && file.Id == _templateFileToExclude.Id)
                    {
                        continue;
                    }

                    yield return file;
                }
            }
        }
    }
}
