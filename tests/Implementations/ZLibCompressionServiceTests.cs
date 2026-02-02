using System.IO.Compression;
using codecrafters_git.Abstractions;
using codecrafters_git.Implementations;
using NSubstitute;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ZLibCompressionServiceTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();

    [Fact]
    public void GetDecompressedObject_ShouldDecompressZlibData()
    {
        // Arrange
        var originalContent = "blob 12\0Hello World!"u8.ToArray();
        var compressedStream = CreateZlibCompressedStream(originalContent);
        const string objectPath = "/test/repo/.git/objects/ab/cd1234";
        _fileSystem.OpenRead(objectPath).Returns(compressedStream);
        var compressionService = new ZLibCompressionService(_fileSystem);

        // Act
        var result = compressionService.GetDecompressedObject(objectPath);

        // Assert
        result.ShouldBe(originalContent);
    }

    [Fact]
    public void SaveCompressedObject_ShouldCreateFileAndWriteCompressedData()
    {
        // Arrange
        var originalContent = "blob 12\0Hello World!"u8.ToArray();
        const string objectPath = "/test/repo/.git/objects/ab/cd1234";
        var writeStream = new MemoryStream();
        _fileSystem.OpenWrite(objectPath).Returns(writeStream);
        var compressionService = new ZLibCompressionService(_fileSystem);

        // Act
        compressionService.SaveCompressedObject(objectPath, originalContent);

        // Assert
        _fileSystem.Received(1).OpenWrite(objectPath);
        var decompressedData = DecompressBytes(writeStream.ToArray());
        decompressedData.ShouldBe(originalContent);
    }

    private static byte[] DecompressBytes(byte[] compressedBytes)
    {
        using var compressedStream = new MemoryStream(compressedBytes);
        using var decompressor = new ZLibStream(compressedStream, CompressionMode.Decompress);
        using var decompressedStream = new MemoryStream();
        decompressor.CopyTo(decompressedStream);

        return decompressedStream.ToArray();
    }

    private static MemoryStream CreateZlibCompressedStream(byte[] content)
    {
        var compressedStream = new MemoryStream();
        using (var zlibStream = new ZLibStream(compressedStream, CompressionMode.Compress, true))
        {
            zlibStream.Write(content, 0, content.Length);
        }

        compressedStream.Position = 0;
        return compressedStream;
    }
}