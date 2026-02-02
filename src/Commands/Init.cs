using codecrafters_git.Abstractions;

namespace codecrafters_git.Commands;

[GitCommand("init")]
public class Init(IRepositoryFactory repoFactory, IFileSystem fileSystem, IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                output.WriteLine("Usage: init [directory]");
                return;
        }

        var repo = args.Length > 0
            ? repoFactory.CreateAtSpecificDirectory(args[0])
            : repoFactory.CreateAtCurrentDirectory();

        fileSystem.CreateDirectory(repo.GitDirectory);
        fileSystem.CreateDirectory(repo.ObjectsDirectory);
        fileSystem.CreateDirectory(repo.RefsDirectory);
        fileSystem.WriteAllText(repo.HeadFile, "ref: refs/heads/main\n");
        output.WriteLine($"Initialized empty Git repository in {repo.GitDirectory}/");
    }
}