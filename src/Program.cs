using codecrafters_git;
using codecrafters_git.Abstractions;
using codecrafters_git.Implementations;
using Microsoft.Extensions.DependencyInjection;

if (args.Length < 1)
{
    throw new ArgumentException("No arguments have been provided.");
}

var services = new ServiceCollection();
ConfigureServices(services);
var serviceProvider = services.BuildServiceProvider();

var commandResolver = serviceProvider.GetRequiredService<ICommandResolver>();
var command = commandResolver.Resolve(args[0]);

if (command is null)
{
    throw new ArgumentException($"Unknown command: {args[0]}.");
}

var remainingArgs = args[1..];
command.Execute(remainingArgs);

return;

static void ConfigureServices(IServiceCollection services)
{
    // Infrastructure
    services.AddSingleton<IFileSystem, FileSystem>();
    services.AddSingleton<IRepositoryFactory, RepositoryFactory>();
    services.AddSingleton<IObjectLocator, ObjectLocator>();
    services.AddSingleton<ICompressionService, ZLibCompressionService>();
    services.AddSingleton<IObjectParser, ObjectParser>();
    services.AddSingleton<IObjectBuilder, ObjectBuilder>();
    services.AddSingleton<IOutputWriter, ConsoleOutputWriter>();
    services.AddSingleton<IGitClient, GitHttpClient>();
    services.AddSingleton<IPackParser, PackParser>();

    services.AddHttpClient();

    // Command resolver
    services.AddSingleton<ICommandResolver, CommandResolver>();

    RegisterGitCommands(services);
}

static void RegisterGitCommands(IServiceCollection serviceCollection)
{
    // Automatically register all ICommand implementations with GitCommandAttribute
    var commandTypes = typeof(Program).Assembly.GetTypes()
        .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract &&
                    t.GetCustomAttributes(typeof(GitCommandAttribute), false).Length != 0);

    foreach (var type in commandTypes)
    {
        serviceCollection.AddTransient(type);
    }
}