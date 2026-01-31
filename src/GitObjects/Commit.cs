namespace codecrafters_git.GitObjects;

public class Commit
{
    public string TreeHash { get; set; } = "";
    public string ParentHash { get; set; } = "";
    public string Author { get; set; } = "";
    public string Committer { get; set; } = "";
    public string Message { get; set; } = "";
}