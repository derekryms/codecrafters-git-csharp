namespace codecrafters_git.Abstractions;

public interface ICommand
{
    void Execute(string[] args);
}