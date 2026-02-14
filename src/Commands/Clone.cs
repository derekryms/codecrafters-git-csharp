using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;

namespace codecrafters_git.Commands;

[GitCommand("clone")]
public class Clone(
    IGitClient gitClient,
    IRepositoryFactory repoFactory,
    IFileSystem fileSystem,
    IObjectBuilder objectBuilder,
    IObjectLocator objectLocator,
    IObjectParser objectParser,
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

        var repo = new Init(repoFactory, fileSystem, output).ExecuteWithResult([args[1]]);
        var referenceDiscoverResult = gitClient.DiscoverReferences(args[0]).Result;
        var pack = gitClient.NegotiatePack(args[0], referenceDiscoverResult.HeadHash).Result;

        var typeToHashDict = new Dictionary<string, ObjectType>();
        foreach (var undeltifiedPackObject in pack.UndeltifiedPackObjects)
        {
            var type = undeltifiedPackObject.Type.ToObjectType();
            var objectBytes = objectBuilder.BuildGitObject(type, undeltifiedPackObject.UncompressedData);
            var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
            typeToHashDict[hash] = type;
            var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);
            compressionService.SaveCompressedObject(objectPath, objectBytes);
        }

        foreach (var refDeltaPackObject in pack.RefDeltaPackObjects)
        {
            var baseObjectExist = typeToHashDict.TryGetValue(refDeltaPackObject.BaseObjectSha, out var baseType);
            if (!baseObjectExist)
            {
                throw new Exception($"Base object not found: {refDeltaPackObject.BaseObjectSha}");
            }

            var baseObjectPath = objectLocator.GetGitObjectFilePath(repo, refDeltaPackObject.BaseObjectSha);
            var decompressedBytes = compressionService.GetDecompressedObject(baseObjectPath);
            var gitObject = objectParser.ParseGitObject(decompressedBytes);
            var sourceObject = gitObject.ContentBytes;
            if (sourceObject.Length != refDeltaPackObject.SourceSize)
            {
                throw new Exception(
                    $"Expected decompressed base object size to be {refDeltaPackObject.SourceSize}, but got {sourceObject.Length}");
            }

            var targetObject = new byte[refDeltaPackObject.TargetSize];
            var currentOffset = 0;
            foreach (var instruction in refDeltaPackObject.RefDeltaInstructions)
            {
                switch (instruction)
                {
                    case CopyInstruction copyInstruction:
                        Array.Copy(sourceObject, copyInstruction.Offset, targetObject, currentOffset,
                            copyInstruction.Size);
                        currentOffset += copyInstruction.Size;
                        break;
                    case InsertInstruction insertInstruction:
                        insertInstruction.BytesToCopyToTarget.CopyTo(targetObject, currentOffset);
                        currentOffset += insertInstruction.BytesToCopyToTarget.Length;
                        break;
                }
            }

            var objectBytes = objectBuilder.BuildGitObject(baseType, targetObject);
            var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
            typeToHashDict[hash] = baseType;
            var objectPath = objectLocator.CreateGitObjectDirectory(repo, hash);
            compressionService.SaveCompressedObject(objectPath, objectBytes);
        }

        var headHashType = typeToHashDict[referenceDiscoverResult.HeadHash];
        if (headHashType is not ObjectType.Commit)
        {
            throw new Exception($"Expected head hash to be a commit object, but got {headHashType}");
        }

        var commitObject = objectLocator.GetGitObjectFilePath(repo, referenceDiscoverResult.HeadHash);
        var commitBytes = compressionService.GetDecompressedObject(commitObject);
        var commit = objectParser.ParseGitObject(commitBytes);

        var treeHash = commit.AsciiContent.Split(' ')[1].Split("\n")[0];
        var treeObject = objectLocator.GetGitObjectFilePath(repo, treeHash);
        var treeBytes = compressionService.GetDecompressedObject(treeObject);
        var gitTreeObject = objectParser.ParseGitObject(treeBytes);
        var tree = objectParser.ParseTreeObject(gitTreeObject.ContentBytes);
        CheckoutTree(repo, tree, repo.RepoDirectory);
    }

    private void CheckoutTree(Repository repo, Tree tree, string directory)
    {
        foreach (var treeEntry in tree.TreeEntries)
        {
            if (treeEntry.Mode == TreeEntryMode.Directory)
            {
                var subDirectory = fileSystem.Combine(directory, treeEntry.Name);
                fileSystem.CreateDirectory(subDirectory);
                var objectPath = objectLocator.GetGitObjectFilePath(repo, treeEntry.Hash);
                var decompressedBytes = compressionService.GetDecompressedObject(objectPath);
                var gitObject = objectParser.ParseGitObject(decompressedBytes);
                var subTree = objectParser.ParseTreeObject(gitObject.ContentBytes);
                CheckoutTree(repo, subTree, subDirectory);
            }
            else
            {
                var objectPath = objectLocator.GetGitObjectFilePath(repo, treeEntry.Hash);
                var decompressedBytes = compressionService.GetDecompressedObject(objectPath);
                var gitObject = objectParser.ParseGitObject(decompressedBytes);
                var filePath = fileSystem.Combine(directory, treeEntry.Name);
                fileSystem.WriteAllText(filePath, gitObject.AsciiContent);
            }
        }
    }
}