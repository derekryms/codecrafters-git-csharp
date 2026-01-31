using System.IO.Compression;

namespace codecrafters_git.Implementations;

public static class Helpers
{
    public static byte[] GetDecompressedObject(string objectPath)
    {
        using var fileStream = File.OpenRead(objectPath);
        using var zLibStream = new ZLibStream(fileStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        zLibStream.CopyTo(decompressedStream);

        return decompressedStream.ToArray();
    }
}