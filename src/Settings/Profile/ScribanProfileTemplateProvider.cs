using OwlCore.Storage;
using Scriban;
using Scriban.Runtime;
using System.Reflection;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

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
        var renderResult = await Template.RenderAsync(entity, MemberRenamer);

        var outputFile = await outputFolder.CreateFileAsync(OutputFileName, true, token);

        using var outputStream = await outputFile.OpenWriteAsync(token);
        using var textWriter = new StreamWriter(outputStream);
        await textWriter.WriteAsync(renderResult);
    }

    private static string DefaultMemberRenamer(MemberInfo member) => member.Name;
}
