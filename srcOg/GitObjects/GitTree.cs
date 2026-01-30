using System.Text;

namespace codecrafters_git.srcOg.GitObjects;

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
    
    public GitTree(byte[] content)
    {
        var header = $"tree {content.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        UncompressedDataBytes = [..headerBytes, ..content];
    }

    public byte[] UncompressedDataBytes { get; }
    
    public static List<GitTreeEntry> GetTreeEntries(byte[] uncompressedDataBytes)
    {
        var treeString = Encoding.UTF8.GetString(uncompressedDataBytes);
        
        var treeEntries = new List<GitTreeEntry>();
        var stream = new MemoryStream(uncompressedDataBytes);
        while (stream.ReadByte() is not '\0');

        while (stream.Position < stream.Length)
        {
            byte b;
            var modeList = new List<byte>();
            while ((b = (byte)stream.ReadByte()) is not (byte)' ')
                modeList.Add(b);
            var mode = Encoding.ASCII.GetString(modeList.ToArray());
        
            var nameList = new List<byte>();
            while ((b = (byte)stream.ReadByte()) is not 0)
                nameList.Add(b);
            var name = Encoding.ASCII.GetString(nameList.ToArray());

            var hashBytes = new byte[20];
            stream.ReadExactly(hashBytes);
            var hash = Convert.ToHexStringLower(hashBytes);
        
            treeEntries.Add(new GitTreeEntry(hash, mode, name));
        }

        return treeEntries;
    }
}