using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class HashObject(
    IRepositoryFactory repoFactory,
    IFileSystem fileSystem,
    IObjectLocator objectLocator,
    ICompressionService compressionService,
    IObjectBuilder objectBuilder,
    IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case 0 or > 2:
                output.WriteLine("Usage: hash-object [-w] <file>");
                return;
            case 1 when args[0] is "-w":
                output.WriteLine("Missing file argument.");
                return;
            case 2 when args[0] is not "-w":
                output.WriteLine("Only -w option supported.");
                return;
        }

        var file = args.Length > 1 ? args[1] : args[0];
        var fileBytes = fileSystem.ReadAllBytes(file);
        var objectBytes = objectBuilder.BuildGitObject(ObjectType.Blob, fileBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));

        if (args[0] is "-w")
        {
            var repo = repoFactory.CreateAtCurrentDirectory();
            var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);
            compressionService.SaveCompressedObject(objectPath, objectBytes);
        }

        output.WriteLine(hash);
    }
}