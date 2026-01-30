namespace codecrafters_git.Commands;

public class WriteTree(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        var workingDir = Path.Combine(Directory.GetCurrentDirectory());
        var tree = Helpers.GetTreeRecursive(workingDir);
        var hash = Helpers.Compress(rootDirectory, tree.UncompressedDataBytes);
        Console.Write(hash);
        
        return Task.CompletedTask;
    }
}