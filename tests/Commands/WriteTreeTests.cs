using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class WriteTreeTests
{
    private readonly ICompressionService _compressionService;
    private readonly Repository _mockRepo;
    private readonly IObjectBuilder _objectBuilder;
    private readonly IObjectLocator _objectLocator;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public WriteTreeTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _objectLocator = Substitute.For<IObjectLocator>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectBuilder = Substitute.For<IObjectBuilder>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");

        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);
    }

    [Fact]
    public void Execute_WithInvalidArgs_ShouldWriteHelpfulMessage()
    {
        // Arrange
        string[] args = ["unexpected-arg"];
        var writeTreeCommand =
            new WriteTree(_repoFactory, _objectLocator, _compressionService, _objectBuilder, _outputWriter);

        // Act
        writeTreeCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine("Usage: write-tree");
    }

    [Fact]
    public void Execute_WithoutArgs_ShouldWriteTreeObjectAndOutputHash()
    {
        // Arrange
        string[] args = [];
        var treeBytes = new byte[] { 1, 2, 3 };
        _objectBuilder.BuildTreeObjectContentBytes(_mockRepo.RepoDirectory).Returns(treeBytes);
        var objectBytes = new byte[] { 4, 5, 6 };
        _objectBuilder.BuildGitObject(ObjectType.Tree, treeBytes).Returns(objectBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        const string objectPath = "/test/repo";
        _objectLocator.CreateGitObjectDirectory(_mockRepo, hash).Returns(objectPath);
        var writeTreeCommand =
            new WriteTree(_repoFactory, _objectLocator, _compressionService, _objectBuilder, _outputWriter);

        // Act
        writeTreeCommand.Execute(args);

        // Assert
        _compressionService.Received(1).SaveCompressedObject(objectPath, objectBytes);
        _outputWriter.Received(1).WriteLine(hash);
    }
}