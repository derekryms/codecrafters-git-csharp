using codecrafters_git.Abstractions;

namespace codecrafters_git.Commands;

[GitCommand("ls-tree")]
public class LsTree(
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
            case 0 or > 2:
                output.WriteLine("Usage: ls-tree [--name-only] <tree-hash>");
                return;
            case 1 when args[0] is "--name-only":
                output.WriteLine("Missing tree hash.");
                return;
            case 2 when args[0] is not "--name-only":
                output.WriteLine("Only --name-only option supported.");
                return;
        }

        var treeHash = args.Length > 1 ? args[1] : args[0];
        var repo = repoFactory.CreateAtCurrentDirectory();
        var objectPath = objectLocator.GetGitObjectFilePath(repo, treeHash);
        var objectDecompressedBytes = compressionService.GetDecompressedObject(objectPath);
        var gitObject = objectParser.ParseGitObject(objectDecompressedBytes);
        var tree = objectParser.ParseTreeObject(gitObject.Content);

        if (args[0] is "--name-only")
        {
            foreach (var entry in tree.TreeEntries)
            {
                output.WriteLine(entry.Name);
            }
        }
        else
        {
            foreach (var entry in tree.TreeEntries)
            {
                var modeString = entry.ModeValue.PadLeft(6, '0');
                var typeString = entry.Type.ToString().ToLower();
                output.WriteLine($"{modeString} {typeString} {entry.Hash}     {entry.Name}");
            }
        }
    }
}