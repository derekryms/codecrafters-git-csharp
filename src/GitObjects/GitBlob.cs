using System.Text;

namespace codecrafters_git.GitObjects;

public class GitBlob
{
    public GitBlob(string content)
    {
        var contentBytes = Encoding.UTF8.GetBytes(content);
        var header = $"blob {contentBytes.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        UncompressedDataBytes = [..headerBytes, ..contentBytes];
    }

    public byte[] UncompressedDataBytes { get; }

    public static string GetContent(byte[] uncompressedDataBytes)
    {
        return Encoding.UTF8.GetString(uncompressedDataBytes).Split('\0')[1];
    }
}