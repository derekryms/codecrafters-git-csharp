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
    public string HeadPath => Path.Combine(GitDirectory, "HEAD");
}