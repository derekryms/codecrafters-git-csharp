using System.IO.Compression;
using codecrafters_git.Abstractions;

namespace codecrafters_git.Implementations;

public class ZLibCompressionService(IFileSystem fileSystem) : ICompressionService
{
    public byte[] GetDecompressedObject(string objectPath)
    {
        using var fileStream = fileSystem.OpenRead(objectPath);
        using var decompressor = new ZLibStream(fileStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        decompressor.CopyTo(decompressedStream);

        return decompressedStream.ToArray();
    }

    public void SaveCompressedObject(string objectPath, byte[] objectBytes)
    {
        using var fileStream = fileSystem.OpenWrite(objectPath);
        using var compressor = new ZLibStream(fileStream, CompressionMode.Compress);
        compressor.Write(objectBytes);
    }
}