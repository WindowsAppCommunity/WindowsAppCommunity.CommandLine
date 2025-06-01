using System.CommandLine;

namespace WindowsAppCommunity.CommandLine.Project.Category;

/// <summary>
/// Wacsdk category command.
/// </summary>
public class CategoryCommand : Command
{    /// <summary>
    /// Initializes a new instance of the <see cref="CategoryCommand"/> class.
    /// </summary>
    public CategoryCommand(WacsdkCommandConfig config, Option<string> repoOption, Option<string> projectIdOption, Option<string> categoryOption)
        : base("category", "Category get and set commands.")
    {
        AddCommand(new GetCategoryCommand(config, repoOption, projectIdOption));
        AddCommand(new SetCategoryCommand(config, repoOption, projectIdOption, categoryOption));
    }
}
