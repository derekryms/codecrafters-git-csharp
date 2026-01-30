namespace codecrafters_git.srcOg.Commands;

public interface ICommand
{
    Task Run(string[] args);
}