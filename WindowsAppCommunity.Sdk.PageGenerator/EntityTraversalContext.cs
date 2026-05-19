using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using OwlCore.Storage;

namespace WindowsAppCommunity.Sdk.PageGenerator;

internal class EntityTraversalContext(IModifiableFolder destinationFolder)
{
    private readonly ConcurrentBag<IFile> _imageFiles = [];
    
    private ConcurrentBag<IFile>? _copiedImageFiles;

    public void AddImage(IFile file) => _imageFiles.Add(file);

    public async IAsyncEnumerable<IFile> CopyImagesAsync(IProgress<CopyProgressEventArgs> progress,
        [EnumeratorCancellation] CancellationToken token = default)
    {
        if (_copiedImageFiles != null)
        {
            foreach (var copiedFile in _copiedImageFiles)
                yield return copiedFile;
            yield break;
        }

        _copiedImageFiles = [];
        var completedFileCount = 0;
        foreach (var imageFile in _imageFiles)
        {
            token.ThrowIfCancellationRequested();

            progress.Report(new CopyProgressEventArgs(_imageFiles.Count, completedFileCount++, imageFile));
            
            var copiedFile = await destinationFolder.CreateCopyOfAsync(imageFile, true, token);
            _copiedImageFiles.Add(copiedFile);
            yield return copiedFile;
        }
    }
}

internal record CopyProgressEventArgs(int TotalFileCount, int CompletedFileCount, IFile CurrentFile)
{
    /// <summary>
    /// The progress of files copied as a percentage.
    /// </summary>
    /// <returns>
    /// A value between 0 and 100.
    /// </returns>
    public double PercentComplete => (double)CompletedFileCount * 100 / TotalFileCount;
}