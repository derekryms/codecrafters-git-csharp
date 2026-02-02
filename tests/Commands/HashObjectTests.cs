using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class HashObjectTests
{
    private readonly ICompressionService _compressionService;
    private readonly IFileSystem _fileSystem;
    private readonly Repository _mockRepo;
    private readonly IObjectBuilder _objectBuilder;
    private readonly IObjectLocator _objectLocator;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public HashObjectTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _fileSystem = Substitute.For<IFileSystem>();
        _objectLocator = Substitute.For<IObjectLocator>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectBuilder = Substitute.For<IObjectBuilder>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");

        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);
    }

    [Theory]
    [InlineData(new string[0], "Usage: hash-object [-w] <file>")]
    [InlineData(new[] { "option", "fileName", "extraArg" }, "Usage: hash-object [-w] <file>")]
    [InlineData(new[] { "-w" }, "Missing file argument.")]
    [InlineData(new[] { "-t", "fileName" }, "Only -w option supported.")]
    public void Execute_WithInvalidArgs_ShouldWriteHelpfulMessage(string[] args, string helpfulMessage)
    {
        // Arrange
        var hashObjectCommand =
            new HashObject(_repoFactory, _fileSystem, _objectLocator, _compressionService, _objectBuilder,
                _outputWriter);

        // Act
        hashObjectCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine(helpfulMessage);
    }

    [Fact]
    public void Execute_WithOnlyFileName_ShouldReturnHash()
    {
        // Arrange
        const string file = "fileName.txt";
        var args = new[] { file };
        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };
        _fileSystem.ReadAllBytes(file).Returns(fileBytes);
        var objectBytes = new byte[] { 6, 7, 8, 9, 10 };
        _objectBuilder.BuildGitObject(ObjectType.Blob, fileBytes).Returns(objectBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        var hashObjectCommand =
            new HashObject(_repoFactory, _fileSystem, _objectLocator, _compressionService, _objectBuilder,
                _outputWriter);

        // Act
        hashObjectCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine(hash);
    }

    [Fact]
    public void Execute_WithSaveOptionAndFileName_ShouldReturnHashAndSaveFile()
    {
        // Arrange
        const string file = "fileName.txt";
        var args = new[] { "-w", file };
        var fileBytes = new byte[] { 1, 2, 3, 4, 5 };
        _fileSystem.ReadAllBytes(file).Returns(fileBytes);
        var objectBytes = new byte[] { 6, 7, 8, 9, 10 };
        _objectBuilder.BuildGitObject(ObjectType.Blob, fileBytes).Returns(objectBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        const string objectPath = "/test/repo/.git/objects/ab/cdef1234567890";
        _objectLocator.CreateGitObjectDirectory(_mockRepo, hash).Returns(objectPath);
        var hashObjectCommand =
            new HashObject(_repoFactory, _fileSystem, _objectLocator, _compressionService, _objectBuilder,
                _outputWriter);

        // Act
        hashObjectCommand.Execute(args);

        // Assert
        _objectLocator.Received(1).CreateGitObjectDirectory(_mockRepo, hash);
        _compressionService.Received(1).SaveCompressedObject(objectPath, objectBytes);
        _outputWriter.Received(1).WriteLine(hash);
    }
}