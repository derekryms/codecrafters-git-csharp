namespace codecrafters_git;

public class Repository
{
    internal Repository(string repoDirectory)
    {
        GitDirectory = Path.Combine(repoDirectory, ".git");
    }

    public string GitDirectory { get; }
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