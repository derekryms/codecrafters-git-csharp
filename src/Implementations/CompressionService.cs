using System.IO.Compression;
using codecrafters_git.Abstractions;

namespace codecrafters_git.Implementations;

public class CompressionService : ICompressionService
{
    public byte[] GetDecompressedObject(string objectPath)
    {
        using var fileStream = File.OpenRead(objectPath);
        using var zLibStream = new ZLibStream(fileStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        zLibStream.CopyTo(decompressedStream);

        return decompressedStream.ToArray();
    }
}