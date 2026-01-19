using System.Text;

namespace codecrafters_git;

public class GitTree
{
    public GitTree(List<GitTreeEntry> treeEntries)
    {
        byte[] bytes = [];
        foreach (var treeEntry in treeEntries)
        {
            var hashBytes = Convert.FromHexString(treeEntry.Sha1Hash);
            var entryBytes = Encoding.UTF8.GetBytes($"{treeEntry.Mode} {treeEntry.Name}\0");
            bytes = [..bytes, ..entryBytes, ..hashBytes];
        }

        var header = $"tree {bytes.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        UncompressedDataBytes = [..headerBytes, ..bytes];
    }

    public byte[] UncompressedDataBytes { get; }
}