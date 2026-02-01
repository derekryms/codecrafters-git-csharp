using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectLocator(IFileSystem fileSystem) : IObjectLocator
{
    public string GetGitObjectFilePath(Repository repo, string objectHash)
    {
        var objectPath = ComputeGitObjectFilePath(repo, objectHash);
        return !fileSystem.FileExists(objectPath.FullPath)
            ? throw new FileNotFoundException($"Object with hash {objectHash} not found.")
            : objectPath.FullPath;
    }

    public string CreateGitObjectDirectory(Repository repo, string objectHash)
    {
        var objectPath = ComputeGitObjectFilePath(repo, objectHash);
        if (!fileSystem.DirectoryExists(Path.Combine(repo.ObjectsDirectory, objectPath.Directory)))
        {
            fileSystem.CreateDirectory(Path.Combine(repo.ObjectsDirectory, objectPath.Directory));
        }

        return objectPath.FullPath;
    }

    public GitObjectPath ComputeGitObjectFilePath(Repository repo, string objectHash)
    {
        var directory = objectHash[..2];
        var file = objectHash[2..];
        var fullPath = Path.Combine(repo.ObjectsDirectory, directory, file);
        return new GitObjectPath(directory, file, fullPath);
    }
}