using System.Security.Cryptography;
using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class CommitTreeTests
{
    private readonly ICompressionService _compressionService;
    private readonly Repository _mockRepo;
    private readonly IObjectBuilder _objectBuilder;
    private readonly IObjectLocator _objectLocator;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public CommitTreeTests()
    {
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _objectLocator = Substitute.For<IObjectLocator>();
        _compressionService = Substitute.For<ICompressionService>();
        _objectBuilder = Substitute.For<IObjectBuilder>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");

        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);
    }

    [Theory]
    [InlineData(new string[0], "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "1arg" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "1arg", "2arg" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "1arg", "2arg", "3arg" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "1arg", "2arg", "3arg", "4arg" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "hash", "badOption1", "pHash", "badOption2" },
        "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "hash", "badOption", "pHash", "-m" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    [InlineData(new[] { "hash", "-p", "pHash", "badOption" }, "Usage: commit-tree <tree> -p <parent> -m <message>")]
    public void Execute_WithInvalidArgs_ShouldWriteHelpfulMessage(string[] args, string helpfulMessage)
    {
        // Arrange
        var commitTreeCommand = new CommitTree(_repoFactory, _objectLocator, _compressionService, _objectBuilder,
            _outputWriter);

        // Act
        commitTreeCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine(helpfulMessage);
    }

    [Fact]
    public void Execute_WithValidArgs_ShouldCreateCommitObjectAndWriteHash()
    {
        // Arrange
        var args = new[] { "treeHash", "-p", "parentHash", "-m", "Initial commit" };
        var commitBytes = new byte[] { 0x01, 0x02, 0x03 };
        var objectBytes = new byte[] { 0x0A, 0x0B, 0x0C };
        const string objectPath = "/test/repo";
        _objectBuilder.BuildCommitObjectContentBytes(args[0], args[2], args[4]).Returns(commitBytes);
        _objectBuilder.BuildGitObject(ObjectType.Commit, commitBytes).Returns(objectBytes);
        var hash = Convert.ToHexStringLower(SHA1.HashData(objectBytes));
        _objectLocator.CreateGitObjectDirectory(_mockRepo, hash).Returns(objectPath);
        var commitTreeCommand = new CommitTree(_repoFactory, _objectLocator, _compressionService, _objectBuilder,
            _outputWriter);

        // Act
        commitTreeCommand.Execute(args);

        // Assert
        _compressionService.Received(1).SaveCompressedObject(objectPath, objectBytes);
        _outputWriter.Received(1).WriteLine(hash);
    }
}