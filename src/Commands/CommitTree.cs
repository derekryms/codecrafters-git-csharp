using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class CommitTree : ICommand
{
    public void Run(string[] args)
    {
        var treeHash = args[0];
        var option1 = args[1];
        var parentHash = args[2];
        var option2 = args[3];
        var message = args[4];

        if (option1 == "-p" && option2 == "-m")
        {
            var commit = new GitCommit(treeHash, parentHash, message);
            var hash = Helpers.Compress(commit.UncompressedDataBytes);
            Console.Write(hash);
        }
    }
}