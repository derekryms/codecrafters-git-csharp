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