using codecrafters_git.Commands;

namespace codecrafters_git;

public interface ICommandResolver
{
    ICommand? Resolve(string commandName);
}