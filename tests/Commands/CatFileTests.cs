using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class CatFileTests
{
    private readonly ICompressionService _compressionService;
    private readonly Repository _mockRepo;
    private readonly IObjectLocator _objectLocator;
    private readonly IObjectParser _objectParser;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public CatFileTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _objectLocator = Substitute.For<IObjectLocator>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectParser = Substitute.For<IObjectParser>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");

        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);
    }

    [Theory]
    [InlineData([new[] { "tooLittleArgs" }])]
    [InlineData([new[] { "option", "object", "extraArg" }])]
    public void Execute_WithTooManyOrTooLittle_ShouldWriteUsageMessage(string[] args)
    {
        // Arrange
        var catFileCommand =
            new CatFile(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);

        // Act
        catFileCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine("Usage: cat-file (-p | -t) <object>");
    }

    [Fact]
    public void Execute_WithInvalidOption_ShouldWriteOptionMessage()
    {
        // Arrange
        var catFileCommand =
            new CatFile(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);
        var args = new[] { "-x", "object" };

        // Act
        catFileCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine("Only -p and -t options supported.");
    }

    [Theory]
    [InlineData("-p", "file content")]
    [InlineData("-t", "blob")]
    public void Execute_WithValidArgs_ShouldWriteContentForPrintOption(string option, string expectedOutput)
    {
        // Arrange
        var catFileCommand =
            new CatFile(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);
        const string objectHash = "abc123";
        var args = new[] { option, objectHash };
        var objectPath = $"{_mockRepo.GitDirectory}/{objectHash[..2]}/{objectHash[2..]}";
        var decompressedBytes = Array.Empty<byte>();

        _objectLocator.GetGitObjectFilePath(_mockRepo, objectHash).Returns(objectPath);
        _compressionService.GetDecompressedObject(objectPath).Returns(decompressedBytes);
        _objectParser.ParseGitObject(decompressedBytes).Returns((ObjectType.Blob, "file content"));

        // Act
        catFileCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).Write(expectedOutput);
    }
}