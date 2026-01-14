using System.Security.Cryptography;
using System.Text;

namespace codecrafters_git;

public class GitBlob
{
    public GitBlob(string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var header = $"blob\x20{contentBytes.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        UncompressedDataBytes = [..headerBytes, ..contentBytes];
    }

    public byte[] UncompressedDataBytes { get; }

    public static string GetContent(byte[] uncompressedDataBytes)
    {
        return Encoding.UTF8.GetString(uncompressedDataBytes).Split('\0')[1];
    }
}

public class GitTree
{
    public GitTree(List<GitTreeEntry> treeEntries)
    {
        byte[] bytes = [];
        foreach (var treeEntry in treeEntries)
        {
            var hashBytes = Convert.FromHexString(treeEntry.Sha1Hash);
            var entryBytes = Encoding.UTF8.GetBytes($"{treeEntry.Mode}\x20{treeEntry.Name}\0");
            bytes = [..bytes, ..entryBytes, ..hashBytes];
        }
        var header = $"tree\x20{bytes.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        UncompressedDataBytes = [..headerBytes, ..bytes];
    }

    public byte[] UncompressedDataBytes { get; }
}

public record GitTreeEntry(string Sha1Hash, string Mode, string Type, string Name);

public class GitCommit
{
    public GitCommit(string treeSha, string parentCommitSha, string commitMessage)
    {
        var parentCommit = $"parent\x20{parentCommitSha}\n";
        var time = DateTime.UtcNow - DateTime.UnixEpoch;
        var offset = Helpers.TimeSpanToTimezoneOffset(TimeZoneInfo.Utc.BaseUtcOffset);
        var author = "author\x20" + "Code\x20" + "Crafters\x20" + $"<user@cc.com>\x20{time.TotalSeconds:F0}\x20{offset}\n";
        var commiter = "commiter\x20" + "Code\x20" + "Crafters\x20" + $"<user@cc.com>\x20{time.TotalSeconds:F0}\x20{offset}\n\n";
        var sb = new StringBuilder();
        sb.Append(parentCommit).Append(author).Append(commiter).AppendLine(commitMessage);
        var header = $"commit\x20{sb.Length}\0tree\x20{treeSha}\n";
        sb.Insert(0, header);
        UncompressedDataBytes = Encoding.UTF8.GetBytes(sb.ToString());
    }
    
    public byte[] UncompressedDataBytes { get; }
}