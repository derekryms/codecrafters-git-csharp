using System.IO.Compression;
using System.Security.Cryptography;

namespace codecrafters_git;

public static class Helpers
{
    public static string GetSha1Hash(byte[] bytes)
    {
        return Convert.ToHexStringLower(SHA1.HashData(bytes));
    }

    public static byte[] GetDecompressedBytes(string hash)
    {
        var decompressed = new MemoryStream();
        using var compressedFileStream = File.Open($".git/objects/{hash[..2]}/{hash[2..]}", FileMode.Open, FileAccess.Read);
        using var decompressor = new ZLibStream(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(decompressed);

        return decompressed.ToArray();
    }

    public static string Compress(byte[] bytes)
    {
        var hash = GetSha1Hash(bytes);

        var fileDirectory = hash[..2];
        var compressedFileName = hash[2..];
        Directory.CreateDirectory($".git/objects/{fileDirectory}");

        using var compressedFileStream = File.Create($".git/objects/{fileDirectory}/{compressedFileName}");
        using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
        compressor.Write(bytes);
        return hash;
    }

    public static GitTree GetTreeRecursive(string workingDir)
    {
        var treeEntries = new List<GitTreeEntry>();
        var files = Directory.GetFiles(workingDir);
        var dirs = Directory.GetDirectories(workingDir);
        foreach (var file in files)
        {
            var blob = new GitBlob(File.ReadAllText(file));
            var unixFileMode = File.GetUnixFileMode(file);
            var hasExecute = unixFileMode.HasFlag(UnixFileMode.OtherExecute) ||
                             unixFileMode.HasFlag(UnixFileMode.GroupExecute) ||
                             unixFileMode.HasFlag(UnixFileMode.UserExecute);
            var mode = hasExecute ? "100755" : "100644";
            treeEntries.Add(new GitTreeEntry(GetSha1Hash(blob.UncompressedDataBytes), mode, "blob",
                Path.GetFileName(file)));
        }

        foreach (var dir in dirs)
        {
            var name = Path.GetFileName(dir);
            if (name == ".git")
                continue;

            var childTree = GetTreeRecursive(dir);
            treeEntries.Add(new GitTreeEntry(GetSha1Hash(childTree.UncompressedDataBytes), "40000", "tree", name));
        }

        var tree = new GitTree(treeEntries.OrderBy(t => t.Name).ToList());
        return tree;
    }

    public static string TimeSpanToTimezoneOffset(TimeSpan offset)
    {
        var sign = offset.Ticks >= 0 ? '+' : '-';
        var hours = Math.Abs(offset.Hours);
        var minutes = Math.Abs(offset.Minutes);
        return $"{sign}{hours:D2}{minutes:D2}";
    }
}