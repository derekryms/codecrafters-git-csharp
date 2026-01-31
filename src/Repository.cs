namespace codecrafters_git;

public class Repository
{
    private Repository(string repoDirectory)
    {
        GitDirectory = Path.Combine(repoDirectory, ".git");
    }

    public string GitDirectory { get; }
    public string ObjectsDirectory => Path.Combine(GitDirectory, "objects");
    public string RefsDirectory => Path.Combine(GitDirectory, "refs");
    public string HeadPath => Path.Combine(GitDirectory, "HEAD");

    public static Repository CreateAtSpecificDirectory(string specificDirectory)
    {
        return new Repository(Path.Combine(Directory.GetCurrentDirectory(), specificDirectory));
    }

    public static Repository CreateAtCurrentDirectory()
    {
        return CreateAtSpecificDirectory(Directory.GetCurrentDirectory());
    }
}