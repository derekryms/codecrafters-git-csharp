namespace codecrafters_git.Commands;

public class WriteTree : ICommand
{
    public void Run(string[] args)
    {
        var workingDir = Path.Combine(Directory.GetCurrentDirectory());
        var tree = Helpers.GetTreeRecursive(workingDir);
        var hash = Helpers.Compress(tree.UncompressedDataBytes);
        Console.Write(hash);
    }
}