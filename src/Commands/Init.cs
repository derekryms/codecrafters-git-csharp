namespace codecrafters_git.Commands;

public interface ICommand
{
    void Execute(string[] args);
}

public class Init : ICommand
{
    public void Execute(string[] args)
    {
        var repo = args.Length > 0
            ? Repository.CreateAtSpecificDirectory(args[0])
            : Repository.CreateAtCurrentDirectory();

        Directory.CreateDirectory(repo.GitDirectory);
        Directory.CreateDirectory(repo.ObjectsDirectory);
        Directory.CreateDirectory(repo.RefsDirectory);
        File.WriteAllText(repo.HeadPath, "ref: refs/heads/main\n");
        Console.WriteLine($"Initialized empty Git repository in {repo.GitDirectory}/");
    }
}