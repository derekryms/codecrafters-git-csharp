using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

[GitCommand("write-tree")]
public class WriteTree(
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
            case > 0:
                output.WriteLine("Usage: write-tree");
                return;
        }

        var repo = repoFactory.CreateAtCurrentDirectory();
        var treeBytes = objectBuilder.BuildTreeObjectContentBytes(repo.RepoDirectory);
        var objectBytes = objectBuilder.BuildGitObject(ObjectType.Tree, treeBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);

        compressionService.SaveCompressedObject(objectPath, objectBytes);

        output.WriteLine(hash);
    }
}