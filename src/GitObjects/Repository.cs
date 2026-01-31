namespace codecrafters_git.GitObjects;

public class Repository(string repoDirectory)
{
    public string GitDirectory { get; } = Path.Combine(repoDirectory, ".git");
    public string ObjectsDirectory => Path.Combine(GitDirectory, "objects");
    public string RefsDirectory => Path.Combine(GitDirectory, "refs");
    public string HeadFile => Path.Combine(GitDirectory, "HEAD");

    private string ComputeGitObjectFilePath(string objectHash)
    {
        var directory = objectHash[..2];
        var file = objectHash[2..];
        return Path.Combine(ObjectsDirectory, directory, file);
    }

    public string GetGitObjectFilePath(string objectHash)
    {
        var objectPath = ComputeGitObjectFilePath(objectHash);
        return !File.Exists(objectPath)
            ? throw new FileNotFoundException($"Object with hash {objectHash} not found.")
            : objectPath;
    }
}