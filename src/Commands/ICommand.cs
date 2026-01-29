namespace codecrafters_git.Commands;

public interface ICommand
{
    Task Run(string[] args);
}