using OwlCore.Diagnostics;
using WindowsAppCommunity.CommandLine;
using System.CommandLine;
using OwlCore.Nomad.Kubo;
using OwlCore.Kubo;
using OwlCore.Storage.System.IO;
using WindowsAppCommunity.CommandLine.Repo;
using WindowsAppCommunity.CommandLine.User;
using WindowsAppCommunity.CommandLine.Project;
using WindowsAppCommunity.CommandLine.Publisher;

// Logging
var startTime = DateTime.Now;
Logger.MessageReceived += Logger_MessageReceived;
void Logger_MessageReceived(object? sender, LoggerMessageEventArgs e)
{
    if (e.Level == LogLevel.Trace)
        return;

    Console.WriteLine($"+{Math.Round((DateTime.Now - startTime).TotalMilliseconds)}ms {Path.GetFileNameWithoutExtension(e.CallerFilePath)} {e.CallerMemberName}  [{e.Level}] {e.Exception} {e.Message}");
}

// Capture and log unhandled or uncaught exceptions.
AppDomain.CurrentDomain.UnhandledException += (object sender, UnhandledExceptionEventArgs e) => Logger.LogError(e.ExceptionObject?.ToString() ?? "Error message not found", e.ExceptionObject as Exception);

// FirstChanceException captures all exceptions that are thrown, even if they are caught later.
//AppDomain.CurrentDomain.FirstChanceException += (object? sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e) => Logger.LogError(e.Exception?.ToString() ?? "Error message not found", e.Exception);

// UnobservedTaskException captures non-awaited (fire-and-forget) task exceptions.
//TaskScheduler.UnobservedTaskException += (object? sender, UnobservedTaskExceptionEventArgs e) => Logger.LogError(e.Exception?.ToString() ?? "Error message not found", e.Exception);

// Cancellation
var cancellationTokenSource = new CancellationTokenSource();
var cancellationToken = cancellationTokenSource.Token;
Console.CancelKeyPress += (sender, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellationTokenSource.Cancel();
};

// Storage setup
var userProfileFolder = new SystemFolder(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
var wacsdkRepoFolder = (SystemFolder)await userProfileFolder.CreateFolderAsync(".wacsdk", overwrite: false, cancellationToken);

// Dedicated ipfs repo for testing in.
var kuboRepoFolder = (SystemFolder)await wacsdkRepoFolder.CreateFolderAsync(".ipfs", overwrite: false, cancellationToken);

// Bootstrap and start Kubo
var kubo = new KuboBootstrapper(kuboRepoFolder.Path)
{
    GatewayUri = new Uri("http://127.0.0.1:8025"),
    ApiUri = new Uri("http://127.0.0.1:5025"),
    GatewayUriMode = ConfigMode.OverwriteExisting,
    ApiUriMode = ConfigMode.OverwriteExisting,
    LaunchConflictMode = BootstrapLaunchConflictMode.Attach,
    RoutingMode = DhtRoutingMode.None,
};
await kubo.StartAsync(cancellationToken);

// Command data / config
var config = new WacsdkCommandConfig
{
    CancellationToken = cancellationToken,
    KuboOptions = new KuboOptions
    {
        IpnsLifetime = TimeSpan.FromDays(1),
        ShouldPin = false,
        UseCache = false,
    },
    Client = kubo.Client,
    RepositoryStorage = wacsdkRepoFolder,
};

// Entry
var rootCommand = new RootCommand("Commands for interacting with the Windows App Community SDK.");
rootCommand.AddCommand(new WacsdkRepoCommands(config));

// Determines the Nomad repository used for creating, storing and retrieving WACSDK data.
// This is a required option for commands that interact with WACSDK data.
var repoOption = new Option<string>(name: "--repo-id", () => "default", description: "The ID of the WACSDK repository.")
{
    IsRequired = true,
};

rootCommand.AddCommand(new WacsdkUserCommands(config, repoOption));
rootCommand.AddCommand(new WacsdkProjectCommands(config, repoOption));
rootCommand.AddCommand(new WacsdkPublisherCommands(config, repoOption));

await rootCommand.InvokeAsync(args);