using System.IO.Compression;

namespace codecrafters_git;

public static class Helpers
{
    public static byte[] GetDecompressedBytes(string hash)
    {
        var decompressed = new MemoryStream();
        using var compressedFileStream = File.Open($".git/objects/{hash[..2]}/{hash[2..]}", FileMode.Open, FileAccess.Read);
        using var decompressor = new ZLibStream(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(decompressed);

        return decompressed.ToArray();
    }
}