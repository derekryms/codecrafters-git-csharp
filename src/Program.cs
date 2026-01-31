using codecrafters_git;
using codecrafters_git.Commands;
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

    // Command resolver
    services.AddSingleton<ICommandResolver, CommandResolver>();

    // Commands
    services.AddTransient<Init>();
    services.AddTransient<CatFile>();
}