using System.IO.Compression;
using codecrafters_git.Abstractions;

namespace codecrafters_git.Implementations;

public class ZLibCompressionService(IFileSystem fileSystem) : ICompressionService
{
    public byte[] GetDecompressedObject(string objectPath)
    {
        using var fileStream = fileSystem.OpenRead(objectPath);
        using var zLibStream = new ZLibStream(fileStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        zLibStream.CopyTo(decompressedStream);

        return decompressedStream.ToArray();
    }
}