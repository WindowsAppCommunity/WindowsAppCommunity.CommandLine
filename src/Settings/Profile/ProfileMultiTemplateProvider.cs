using CommunityToolkit.Diagnostics;
using OwlCore.Storage;
using WindowsAppCommunity.Sdk;

namespace WindowsAppCommunity.CommandLine.Settings.Profile;

public class ProfileMultiTemplateProvider<TEntity> : IProfileTemplateProvider<TEntity>
    where TEntity : IReadOnlyEntity
{
    public List<IProfileTemplateProvider<TEntity>> Templates { get; init; } = [];

    public async Task ApplyTemplate(TEntity entity, IModifiableFolder outputFolder, CancellationToken token = default)
    {
        Guard.IsNotNull(entity);
        Guard.IsNotNull(outputFolder);

        foreach (var template in Templates)
        {
            await template.ApplyTemplate(entity, outputFolder, token);

            token.ThrowIfCancellationRequested();
        }
    }
}
