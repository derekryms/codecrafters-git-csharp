using codecrafters_git.Abstractions;
using Microsoft.Extensions.Logging;

namespace codecrafters_git.Commands;

public class Init(IRepositoryFactory repoFactory, IFileSystem fileSystem, ILogger<Init> logger) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case > 1:
                logger.LogWarning("Usage: init [directory]");
                return;
        }

        var repo = args.Length > 0
            ? repoFactory.CreateAtSpecificDirectory(args[0])
            : repoFactory.CreateAtCurrentDirectory();

        fileSystem.CreateDirectory(repo.GitDirectory);
        fileSystem.CreateDirectory(repo.ObjectsDirectory);
        fileSystem.CreateDirectory(repo.RefsDirectory);
        fileSystem.WriteAllText(repo.HeadFile, "ref: refs/heads/main\n");
        logger.LogInformation("Initialized empty Git repository in {GitDirectory}/", repo.GitDirectory);
    }
}