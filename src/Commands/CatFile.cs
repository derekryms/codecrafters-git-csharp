using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class CatFile(IRepositoryFactory repoFactory) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case < 2 or > 2:
                Console.WriteLine("Usage: cat-file (-p | -t) <object>");
                return;
            case 2 when args[0] is not "-p" and not "-t":
                Console.WriteLine("Only -p and -t options supported.");
                return;
        }

        var repo = repoFactory.CreateAtCurrentDirectory();
        var objectPath = repo.GetGitObjectFilePath(args[1]);
        var decompressedBytes = Helpers.GetDecompressedObject(objectPath);
        var (type, content) = GitObjectHelpers.ParseGitObject(decompressedBytes);

        switch (args[0])
        {
            case "-p":
                Console.WriteLine(content);
                break;
            case "-t":
                Console.WriteLine(type.ToString().ToLower());
                break;
        }
    }
}