using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectLocator(IFileSystem fileSystem) : IObjectLocator
{
    public string GetGitObjectFilePath(Repository repo, string objectHash)
    {
        var objectPath = ComputeGitObjectFilePath(repo, objectHash);
        return !fileSystem.FileExists(objectPath)
            ? throw new FileNotFoundException($"Object with hash {objectHash} not found.")
            : objectPath;
    }

    private static string ComputeGitObjectFilePath(Repository repo, string objectHash)
    {
        var directory = objectHash[..2];
        var file = objectHash[2..];
        return Path.Combine(repo.ObjectsDirectory, directory, file);
    }
}