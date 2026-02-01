using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace codecrafters_git.Implementations;

public class CommandResolver(IServiceProvider serviceProvider) : ICommandResolver
{
    private static readonly Dictionary<string, Type> CommandTypes = new()
    {
        { "init", typeof(Init) },
        { "cat-file", typeof(CatFile) },
        { "hash-object", typeof(HashObject) }
    };

    public ICommand? Resolve(string commandName)
    {
        return CommandTypes.TryGetValue(commandName, out var commandType)
            ? serviceProvider.GetRequiredService(commandType) as ICommand
            : null;
    }
}