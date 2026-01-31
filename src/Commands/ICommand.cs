namespace codecrafters_git.Commands;

public interface ICommand
{
    void Execute(string[] args);
}