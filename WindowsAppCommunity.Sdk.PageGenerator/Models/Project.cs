namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record Project
{
    // TODO: Complete project model
    public Entity Entity { get; init; } 
    
    public string Category { get; init; }
    
    public string Url { get; init; }
    
    public static async Task<Project> CreateAsync(IReadOnlyProject sdkProject, CancellationToken token = default)
    {
        return new Project
        {
            Entity = await Entity.CreateAsync(sdkProject, token),
            Category = sdkProject.Category,
            Url = $"https://ipfs.io/ipns/{sdkProject.Id}",
        };
    }
    
    public static async Task<List<Project>> CreateAsync(IReadOnlyProjectCollection sdkProjectCollection, CancellationToken token = default)
    {
        List<Project> projects = [];
        await foreach (var sdkProject in sdkProjectCollection.GetProjectsAsync(token))
        {
            var project = await CreateAsync(sdkProject, token);
            projects.Add(project);
        }
        return projects;
    }
}