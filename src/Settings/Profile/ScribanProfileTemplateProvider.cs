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
    public required string OutputFileName { get; init; }

    public required Template Template { get; init; }

    public MemberRenamerDelegate MemberRenamer { get; init; } = DefaultMemberRenamer;

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
