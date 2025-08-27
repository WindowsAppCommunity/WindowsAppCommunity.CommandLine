using System.Reflection;
using System.Text;
using OwlCore.Storage;
using Scriban;
using Scriban.Runtime;

namespace WindowsAppCommunity.Sdk.PageGenerator.Templating;

/// <summary>
/// An interface for generating content from an <see cref="IReadOnlyEntity"/> using a
/// <see href="https://github.com/scriban/scriban">Scriban</see> scripted template.
/// </summary>
public class ScribanProfileTemplateProvider<TEntity> : IProfileTemplateProvider<TEntity>
    where TEntity : IReadOnlyEntity
{
    /// <summary>
    /// The name of the file to output to.
    /// </summary>
    public required string OutputFileName { get; init; }

    /// <summary>
    /// The Scriban template to use.
    /// </summary>
    public required Template Template { get; init; }

    /// <summary>
    /// The delegate to use to rename members made accessible from the template scripts.
    /// </summary>
    /// <remarks>
    /// By default, member names are not renamed at all.
    /// </remarks>
    public MemberRenamerDelegate MemberRenamer { get; init; } = DefaultMemberRenamer;

    /// <inheritdoc/>
    public async Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default)
    {
        var model = await GetModelAsync(entity, token);
        var renderResult = await Template.RenderAsync(model, MemberRenamer);

        var outputFile = await outputFolder.CreateFileAsync(OutputFileName, true, token);

        await using var outputStream = await outputFile.OpenWriteAsync(token);
        await using var textWriter = new StreamWriter(outputStream);
        await textWriter.WriteAsync(renderResult);
    }

    /// <summary>
    /// Gets the model passed to the Scriban template for rendering.
    /// </summary>
    /// <param name="entity">The entity to generate a model from.</param>
    /// <returns>A model suitable for rendering templates.</returns>
    protected virtual async Task<object> GetModelAsync(TEntity entity, CancellationToken token) => entity;

    /// <summary>
    /// Creates a template provider from a file containing a Scriban template.
    /// </summary>
    /// <param name="templateFile">The file with the template.</param>
    /// <param name="encoding">The text encoding of the file.</param>
    /// <param name="outputFileName">The name of the file to output to.</param>
    /// <param name="token">A token to cancel the operation.</param>
    /// <returns>A task that represents the operation.</returns>
    public static async Task<ScribanProfileTemplateProvider<TEntity>> CreateFromFileAsync(IFile templateFile, Encoding encoding, string outputFileName, CancellationToken token = default)
    {
        return new ScribanProfileTemplateProvider<TEntity>
        {
            OutputFileName = outputFileName,
            Template = await ParseTemplateFromFileAsync(templateFile, encoding, token),
        };
    }

    /// <inheritdoc cref="CreateFromFileAsync(IFile, Encoding, string, CancellationToken)"/>
    public static async Task<ScribanProfileTemplateProvider<TEntity>> CreateFromFileAsync(IFile templateFile, string outputFileName, CancellationToken token = default)
    {
        return await CreateFromFileAsync(templateFile, Encoding.UTF8, outputFileName, token);
    }

    protected static async Task<Template> ParseTemplateFromFileAsync(IFile templateFile, Encoding encoding, CancellationToken token)
    {
        var templateText = await templateFile.ReadTextAsync(encoding, token);
        return Template.Parse(templateText);
    }

    private static string DefaultMemberRenamer(MemberInfo member) => member.Name;
}
