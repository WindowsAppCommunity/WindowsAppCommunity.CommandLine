using OwlCore.Storage;
using System.Text;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

public class ScribanUserProfileTemplateProvider : ScribanProfileTemplateProvider<IReadOnlyUser>
{
    /// <inheritdoc/>
    protected override async Task<object> GetModelAsync(IReadOnlyUser entity, CancellationToken token)
    {
        return new
        {
            User = entity,

            Connections = await entity.GetConnectionsAsync(token).ToListAsync(token),
            Projects = await entity.GetProjectsAsync(token).ToListAsync(token),
            Publishers = await entity.GetPublishersAsync(token).ToListAsync(token),

            // TODO: How can an HTML template show images from arbitrary files?
            //ImageFiles = await entity.GetImageFilesAsync(token).ToListAsync(token),
        };
    }

    /// <summary>
    /// Creates a template provider from a file containing a Scriban template for use with <see cref="IReadOnlyUser"/>s.
    /// </summary>
    /// <param name="templateFile">The file with the template.</param>
    /// <param name="encoding">The text encoding of the file.</param>
    /// <param name="outputFileName">The name of the file to output to.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public static async Task<ScribanUserProfileTemplateProvider> CreateFromFileAsync(IFile templateFile, Encoding encoding, string outputFileName, CancellationToken token = default)
    {
        return new()
        {
            OutputFileName = outputFileName,
            Template = await ParseTemplateFromFileAsync(templateFile, encoding, token),
        };
    }

    /// <inheritdoc cref="CreateFromFileAsync(IFile, Encoding, string, CancellationToken)"/>
    public static async Task<ScribanUserProfileTemplateProvider> CreateFromFileAsync(IFile templateFile, string outputFileName, CancellationToken token = default)
    {
        return await CreateFromFileAsync(templateFile, Encoding.UTF8, outputFileName, token);
    }
}
