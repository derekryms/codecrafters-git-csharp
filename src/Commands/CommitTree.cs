using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

[GitCommand("commit-tree")]
public class CommitTree(
    IRepositoryFactory repoFactory,
    IObjectLocator objectLocator,
    ICompressionService compressionService,
    IObjectBuilder objectBuilder,
    IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case not 5:
            case 5 when args[1] != "-p" && args[3] != "-m":
                output.WriteLine("Usage: commit-tree <tree> -p <parent> -m <message>");
                return;
        }

        var commitBytes = objectBuilder.BuildCommitObjectContentBytes(args[0], args[2], args[4]);
        var objectBytes = objectBuilder.BuildGitObject(ObjectType.Commit, commitBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        var repo = repoFactory.CreateAtCurrentDirectory();
        var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);
        compressionService.SaveCompressedObject(objectPath, objectBytes);

        output.WriteLine(hash);
    }
}