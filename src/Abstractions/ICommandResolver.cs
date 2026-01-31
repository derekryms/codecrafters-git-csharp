namespace codecrafters_git.Abstractions;

public interface ICommandResolver
{
    ICommand? Resolve(string commandName);
}