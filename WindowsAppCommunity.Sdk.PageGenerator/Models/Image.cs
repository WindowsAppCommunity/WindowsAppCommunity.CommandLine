using OwlCore.Kubo;
using OwlCore.Storage;
using OwlCore.Storage.System.Net.Http;

namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record Image(string Url, IFile File)
{
    public static async Task<Image> CreateAsync(IFile file, CancellationToken token = default)
    {
        string url;
        
        if (file is IGetCid ipfsFile)
        {
            // Generate gateway link for IPFS items
            // TODO: Use WinAppComm gateway (or no gateway at all)
            var cid = await ipfsFile.GetCidAsync(token);
            url = $"https://ipfs.io/ipfs/{cid}";
        }
        else if (file is HttpFile httpFile)
        {
            // Use HTTP links directly
            url = httpFile.Uri.ToString();
        }
        else
        {
            await using var stream = await file.OpenReadAsync(token);
            
            var length = long.MaxValue;
            try
            {
                length = stream.Length;
            }
            catch {}

            var maxEncodedLength = Math.Ceiling(length * 8.0 / 6.0);
            
            // Encode small images as base64. The length limit of a data URI
            // varies across browsers and platforms, but the smallest appears
            // to be Chrome with a 2MB limit.
            if (true)//maxEncodedLength < 1_999_000)
            {
                // See https://developer.mozilla.org/en-US/docs/Web/URI/Reference/Schemes/data#syntax
                // and https://superuser.com/questions/979135/is-there-a-generic-mime-type-for-all-image-files
                var base64 = await stream.Base64EncodeAsync(token);
                url = $"data:image/xyz;base64,{base64}";
            }
            else
            {
                // TODO: Support large images. Consider copying them to the
                // page output directory, then using a relative link here.
                throw new NotSupportedException($"Image '{file.Id}' may require a URL of {maxEncodedLength} bytes, which is too large to be used at this time.");
            }
        }
        
        return new Image(url, file);
    }

    public static async Task<List<Image>> CreateAsync(IAsyncEnumerable<IFile> sdkImages, CancellationToken token = default)
    {
        List<Image> images = [];
        await foreach (var sdkImage in sdkImages.WithCancellation(token))
        {
            var image = await CreateAsync(sdkImage, token);
            images.Add(image);
        }
        return images;
    }
}