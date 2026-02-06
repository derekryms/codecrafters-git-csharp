namespace codecrafters_git.GitObjects;

public class Repository(string repoDirectory)
{
    public string RepoDirectory { get; } = repoDirectory;
    public string GitDirectory { get; } = Path.Combine(repoDirectory, ".git");
    public string ObjectsDirectory => Path.Combine(GitDirectory, "objects");
    public string RefsDirectory => Path.Combine(GitDirectory, "refs");
    public string HeadFile => Path.Combine(GitDirectory, "HEAD");
}