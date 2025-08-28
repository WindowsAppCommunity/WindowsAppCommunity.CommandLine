namespace WindowsAppCommunity.Sdk.PageGenerator.Models;

public record ProjectRole
{
    public Project Project { get; init; }
    
    public Role Role { get; init; }

    public static async Task<ProjectRole> CreateAsync(IReadOnlyProjectRole sdkProjectRole, CancellationToken token = default)
    {
        return new ProjectRole
        {
            Project = await Project.CreateAsync(sdkProjectRole, token),
            Role = sdkProjectRole.Role
        };
    }
    
    public static async Task<List<ProjectRole>> CreateAsync(IReadOnlyProjectRoleCollection sdkProjectRoleCollection, CancellationToken token = default)
    {
        List<ProjectRole> projectRoles = [];
        await foreach (var sdkProjectRole in sdkProjectRoleCollection.GetProjectsAsync(token))
        {
            var projectRole = await CreateAsync(sdkProjectRole, token);
            projectRoles.Add(projectRole);
        }
        return projectRoles;
    }
}