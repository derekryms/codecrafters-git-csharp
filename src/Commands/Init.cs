namespace codecrafters_git.Commands;

public class Init(RepositoryFactory repoFactory) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                Console.WriteLine("Usage: init [directory]");
                return;
        }

        var repo = args.Length > 0
            ? repoFactory.CreateAtSpecificDirectory(args[0])
            : repoFactory.CreateAtCurrentDirectory();

        Directory.CreateDirectory(repo.GitDirectory);
        Directory.CreateDirectory(repo.ObjectsDirectory);
        Directory.CreateDirectory(repo.RefsDirectory);
        File.WriteAllText(repo.HeadFile, "ref: refs/heads/main\n");
        Console.WriteLine($"Initialized empty Git repository in {repo.GitDirectory}/");
    }
}