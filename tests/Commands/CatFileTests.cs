using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class CatFileTests
{
    private readonly ICompressionService _compressionService;
    private readonly IObjectParser _objectParser;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public CatFileTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectParser = Substitute.For<IObjectParser>();
        _outputWriter = Substitute.For<IOutputWriter>();

        _repoFactory.CreateAtCurrentDirectory().Returns(new Repository("/test/repo"));
    }

    [Theory]
    [InlineData([new[] { "tooLittleArgs" }])]
    [InlineData([new[] { "option", "object", "extraArg" }])]
    public void Execute_WithTooManyOrTooLittle_ShouldWriteUsageMessage(string[] args)
    {
        // Arrange
        var catFileCommand = new CatFile(_repoFactory, _compressionService, _objectParser, _outputWriter);

        // Act
        catFileCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine("Usage: cat-file (-p | -t) <object>");
    }

    [Fact]
    public void Execute_WithInvalidOption_ShouldWriteOptionMessage()
    {
        // Arrange
        var catFileCommand = new CatFile(_repoFactory, _compressionService, _objectParser, _outputWriter);
        var args = new[] { "-x", "object" };

        // Act
        catFileCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine("Only -p and -t options supported.");
    }
    //
    // [Fact]
    // public void Execute_WithValidArgs_ShouldWriteContentForPrintOption()
    // {
    //     // Arrange
    //     var catFileCommand = new CatFile(_repoFactory, _compressionService, _objectParser, _outputWriter);
    //     var args = new[] { "-p", "object" };
    //     var decompressedBytes = new byte[]
    //     {
    //         /* ... */
    //     };
    //     _compressionService.GetDecompressedObject(Arg.Any<string>()).Returns(decompressedBytes);
    //     _objectParser.ParseGitObject(decompressedBytes).Returns((ObjectType.Blob, "file content"));
    //
    //     // Act
    //     catFileCommand.Execute(args);
    //
    //     // Assert
    //     _outputWriter.Received(1).Write("file content");
    // }
}