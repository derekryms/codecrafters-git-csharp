using System.Security.Cryptography;
using System.Text;

namespace codecrafters_git;

public class GitBlob
{
    private readonly string _uncompressedData;

    public GitBlob(string content)
    {
        var header = $"blob\x20{content.Length}\0";
        _uncompressedData = $"{header}{content}";
    }

    public byte[] UncompressedDataBytes => Encoding.UTF8.GetBytes(_uncompressedData);

    public string GetSha1Hash()
    {
        return Convert.ToHexStringLower(SHA1.HashData(UncompressedDataBytes));
    }

    public static string GetContent(byte[] uncompressedDataBytes)
    {
        return Encoding.UTF8.GetString(uncompressedDataBytes).Split('\0')[1];
    }

}

public record GitTree(List<GitTreeEntry> TreeEntries);

public record GitTreeEntry(string Sha1Hash, string Mode, string Type, string Name);