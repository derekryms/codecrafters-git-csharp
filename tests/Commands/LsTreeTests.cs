using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class LsTreeTests
{
    private readonly ICompressionService _compressionService;
    private readonly IFileSystem _fileSystem;
    private readonly Repository _mockRepo;
    private readonly IObjectBuilder _objectBuilder;
    private readonly IObjectLocator _objectLocator;
    private readonly IObjectParser _objectParser;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public LsTreeTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _fileSystem = Substitute.For<IFileSystem>();
        _objectLocator = Substitute.For<IObjectLocator>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectBuilder = Substitute.For<IObjectBuilder>();
        _objectParser = Substitute.For<IObjectParser>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");

        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);
    }

    [Theory]
    [InlineData(new string[0], "Usage: ls-tree [--name-only] <tree-hash>")]
    [InlineData(new[] { "option", "treeHash", "extraArg" }, "Usage: ls-tree [--name-only] <tree-hash>")]
    [InlineData(new[] { "--name-only" }, "Missing tree hash.")]
    [InlineData(new[] { "--other-option", "treeHash" }, "Only --name-only option supported.")]
    public void Execute_WithInvalidArgs_ShouldWriteHelpfulMessage(string[] args, string helpfulMessage)
    {
        // Arrange
        var lsTreeCommand =
            new LsTree(_repoFactory, _fileSystem, _objectLocator, _compressionService, _objectBuilder, _objectParser,
                _outputWriter);

        // Act
        lsTreeCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine(helpfulMessage);
    }
}