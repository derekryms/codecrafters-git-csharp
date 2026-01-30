using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class CommitTree(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        var treeHash = args[0];
        var option1 = args[1];
        var parentHash = args[2];
        var option2 = args[3];
        var message = args[4];

        if (option1 == "-p" && option2 == "-m")
        {
            var commit = new GitCommit(treeHash, parentHash, message);
            var hash = Helpers.Compress(rootDirectory, commit.UncompressedDataBytes);
            Console.Write(hash);
        }
        
        return Task.CompletedTask;
    }
}