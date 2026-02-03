using codecrafters_git.Abstractions;

namespace codecrafters_git.Commands;

[GitCommand("cat-file")]
public class CatFile(
    IRepositoryFactory repoFactory,
    IObjectLocator objectLocator,
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
        var objectPath = objectLocator.GetGitObjectFilePath(repo, args[1]);
        var decompressedBytes = compressionService.GetDecompressedObject(objectPath);
        var gitObject = objectParser.ParseGitObject(decompressedBytes);
        var blob = objectParser.ParseBlobObject(gitObject.Content);

        switch (args[0])
        {
            case "-p":
                output.Write(blob.AsciiContent);
                break;
            case "-t":
                output.Write(gitObject.Header.Type.ToString().ToLower());
                break;
        }
    }
}