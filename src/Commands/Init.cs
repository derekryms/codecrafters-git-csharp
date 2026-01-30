namespace codecrafters_git.Commands;

public interface ICommand
{
    void Execute(string[] args);
}

public class Init() : ICommand
{
    public void Execute(string[] args)
    {
        var workingDirectory = Directory.GetCurrentDirectory();
        var repoDirectory = args.Length > 0 ? Path.Combine(workingDirectory, args[0]) : workingDirectory;
        var gitDirectory = Path.Combine(repoDirectory, ".git/");
        Directory.CreateDirectory(gitDirectory);
        Directory.CreateDirectory(Path.Combine(gitDirectory, "objects"));
        Directory.CreateDirectory(Path.Combine(gitDirectory, "refs"));
        File.WriteAllText(Path.Combine(gitDirectory, "HEAD"), "ref: refs/heads/main\n");
        Console.WriteLine($"Initialized empty Git repository in {gitDirectory}");
    }
}