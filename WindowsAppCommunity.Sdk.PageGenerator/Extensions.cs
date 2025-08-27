using System.Text;

namespace WindowsAppCommunity.Sdk.PageGenerator;

internal static class Extensions
{
    public static async Task<string> Base64EncodeAsync(this Stream stream, CancellationToken token = default)
    {
        // If it's already loaded into memory, we might as well use it
        if (stream is MemoryStream memoryStream)
        {
            return Convert.ToBase64String(memoryStream.ToArray());
        }
        
        // Allocate a buffer whose data will expand to 1024 bytes
        // when encoded. (768 = 1024 * 6 / 8)
        var buffer = new byte[768];
        var sb = new StringBuilder();
        
        while (true)
        {
            var numBytesRead = await stream.ReadAsync(buffer, token);
            if (numBytesRead == 0)
                break;

            var readBytes = buffer[..numBytesRead];
            var encodedBytes = Convert.ToBase64String(readBytes);
            sb.Append(encodedBytes);
            
            token.ThrowIfCancellationRequested();
        }

        return sb.ToString();
    }
}