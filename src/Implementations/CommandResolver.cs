using System.Reflection;
using codecrafters_git.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace codecrafters_git.Implementations;

public class CommandResolver(IServiceProvider serviceProvider) : ICommandResolver
{
    private static readonly Dictionary<string, Type> CommandTypes;

    static CommandResolver()
    {
        CommandTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(ICommand).IsAssignableFrom(t) && !t.IsAbstract &&
                        t.GetCustomAttribute<GitCommandAttribute>() != null)
            .ToDictionary(
                t => t.GetCustomAttribute<GitCommandAttribute>()!.Name,
                t => t
            );
    }

    public ICommand? Resolve(string commandName)
    {
        return CommandTypes.TryGetValue(commandName, out var commandType)
            ? serviceProvider.GetRequiredService(commandType) as ICommand
            : null;
    }
}