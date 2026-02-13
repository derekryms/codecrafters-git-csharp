using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.Implementations;

namespace codecrafters_git.Commands;

[GitCommand("clone")]
public class Clone(
    IGitClient gitClient,
    IRepositoryFactory repoFactory,
    IFileSystem fileSystem,
    IObjectBuilder objectBuilder,
    IObjectLocator objectLocator,
    ICompressionService compressionService,
    IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case not 2:
                output.WriteLine("Usage: clone <repo> <dir>");
                return;
        }

        // new srcOg.Commands.Clone().Run(args).Wait();
        var repo = new Init(repoFactory, fileSystem, output).ExecuteWithResult([args[1]]);
        var referenceDiscoverResult = gitClient.DiscoverReferences(args[0]).Result;
        var pack = gitClient.NegotiatePack(args[0], referenceDiscoverResult.HeadHash).Result;

        foreach (var undeltifiedPackObject in pack.UndeltifiedPackObjects)
        {
            var objectBytes = objectBuilder.BuildGitObject(undeltifiedPackObject.Type.ToObjectType(),
                undeltifiedPackObject.UncompressedData);
            var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
            var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);
            compressionService.SaveCompressedObject(objectPath, objectBytes);
        }
    }
}