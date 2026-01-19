using System.Text;

namespace codecrafters_git.GitObjects;

public class GitCommit
{
    public GitCommit(string treeSha, string parentCommitSha, string commitMessage)
    {
        var parentCommit = $"parent {parentCommitSha}\n";
        var time = DateTime.UtcNow - DateTime.UnixEpoch;
        var offset = Helpers.TimeSpanToTimezoneOffset(TimeZoneInfo.Utc.BaseUtcOffset);
        var author = $"author Code Crafters <user@cc.com> {time.TotalSeconds:F0} {offset}\n";
        var commiter = $"commiter Code Crafters <user@cc.com> {time.TotalSeconds:F0} {offset}\n\n";
        var sb = new StringBuilder();
        sb.Append(parentCommit).Append(author).Append(commiter).AppendLine(commitMessage);
        var header = $"commit {sb.Length}\0tree {treeSha}\n";
        sb.Insert(0, header);
        UncompressedDataBytes = Encoding.UTF8.GetBytes(sb.ToString());
    }

    public byte[] UncompressedDataBytes { get; }
}