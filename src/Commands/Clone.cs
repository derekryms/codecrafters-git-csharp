namespace codecrafters_git.Commands;

public class Clone : ICommand
{
    public void Run(string[] args)
    {
        var repoUrl = args[0];
        var destination = args[1];

        Directory.CreateDirectory(destination);
        Console.Error.WriteLine($"Cloning {repoUrl} into {destination}");
    }
}