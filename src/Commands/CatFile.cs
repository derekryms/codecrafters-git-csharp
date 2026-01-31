using codecrafters_git.Abstractions;
using Microsoft.Extensions.Logging;

namespace codecrafters_git.Commands;

public class CatFile(
    IRepositoryFactory repoFactory,
    ICompressionService compressionService,
    IObjectParser objectParser,
    ILogger<CatFile> logger) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case < 2 or > 2:
                logger.LogWarning("Usage: cat-file (-p | -t) <object>");
                return;
            case 2 when args[0] is not "-p" and not "-t":
                logger.LogWarning("Only -p and -t options supported.");
                return;
        }

        var repo = repoFactory.CreateAtCurrentDirectory();
        var objectPath = repo.GetGitObjectFilePath(args[1]);
        var decompressedBytes = compressionService.GetDecompressedObject(objectPath);
        var (type, content) = objectParser.ParseGitObject(decompressedBytes);

        switch (args[0])
        {
            case "-p":
                logger.LogInformation("{Content}", content);
                break;
            case "-t":
                logger.LogInformation("{Type}", type.ToString().ToLower());
                break;
        }
    }
}