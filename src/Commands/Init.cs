namespace codecrafters_git.Commands;

public class Init(IRepositoryFactory repoFactory, IFileSystem fileSystem) : ICommand
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

        fileSystem.CreateDirectory(repo.GitDirectory);
        fileSystem.CreateDirectory(repo.ObjectsDirectory);
        fileSystem.CreateDirectory(repo.RefsDirectory);
        fileSystem.WriteAllText(repo.HeadFile, "ref: refs/heads/main\n");
        Console.WriteLine($"Initialized empty Git repository in {repo.GitDirectory}/");
    }
}