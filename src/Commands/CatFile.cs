using codecrafters_git.Abstractions;

namespace codecrafters_git.Commands;

public class CatFile(
    IRepositoryFactory repoFactory,
    ICompressionService compressionService,
    IObjectParser objectParser,
    IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case < 2 or > 2:
                output.WriteLine("Usage: cat-file (-p | -t) <object>");
                return;
            case 2 when args[0] is not "-p" and not "-t":
                output.WriteLine("Only -p and -t options supported.");
                return;
        }

        var repo = repoFactory.CreateAtCurrentDirectory();
        var objectPath = repo.GetGitObjectFilePath(args[1]);
        var decompressedBytes = compressionService.GetDecompressedObject(objectPath);
        var (type, content) = objectParser.ParseGitObject(decompressedBytes);

        switch (args[0])
        {
            case "-p":
                output.Write(content);
                break;
            case "-t":
                output.Write(type.ToString().ToLower());
                break;
        }
    }
}