using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class LsTreeTests
{
    private readonly ICompressionService _compressionService;
    private readonly Repository _mockRepo;
    private readonly IObjectLocator _objectLocator;
    private readonly IObjectParser _objectParser;
    private readonly IOutputWriter _outputWriter;
    private readonly IRepositoryFactory _repoFactory;

    public LsTreeTests()
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
    [InlineData(new string[0], "Usage: ls-tree [--name-only] <tree-hash>")]
    [InlineData(new[] { "option", "treeHash", "extraArg" }, "Usage: ls-tree [--name-only] <tree-hash>")]
    [InlineData(new[] { "--name-only" }, "Missing tree hash.")]
    [InlineData(new[] { "--other-option", "treeHash" }, "Only --name-only option supported.")]
    public void Execute_WithInvalidArgs_ShouldWriteHelpfulMessage(string[] args, string helpfulMessage)
    {
        // Arrange
        var lsTreeCommand = new LsTree(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);

        // Act
        lsTreeCommand.Execute(args);

        // Assert
        _outputWriter.Received(1).WriteLine(helpfulMessage);
    }

    [Fact]
    public void Execute_WithNameOnlyOption_ShouldListOnlyNames()
    {
        // Arrange
        const string treeHash = "abc123";
        var args = new[] { "--name-only", treeHash };
        const string objectPath = "test/path";
        var decompressedBytes = "test bytes"u8.ToArray();
        var contentBytes = "test content"u8.ToArray();
        var gitObject = new GitObject(new ObjectHeader(ObjectType.Tree, contentBytes.Length), contentBytes);
        var tree = new Tree([
            new TreeEntry(TreeEntryMode.RegularFile, "someHash1", "file1.txt"),
            new TreeEntry(TreeEntryMode.ExecutableFile, "someHash2", "script.sh"),
            new TreeEntry(TreeEntryMode.SymbolicLink, "someHash3", "link.lnk"),
            new TreeEntry(TreeEntryMode.Directory, "someHash4", "src")
        ]);

        _objectLocator.GetGitObjectFilePath(_mockRepo, treeHash).Returns(objectPath);
        _compressionService.GetDecompressedObject(objectPath).Returns(decompressedBytes);
        _objectParser.ParseGitObject(decompressedBytes).Returns(gitObject);
        _objectParser.ParseTreeObject(gitObject.Content).Returns(tree);
        var lsTreeCommand = new LsTree(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);

        // Act
        lsTreeCommand.Execute(args);

        // Assert
        foreach (var entry in tree.TreeEntries)
        {
            _outputWriter.Received(1).WriteLine(entry.Name);
        }
    }

    [Fact]
    public void Execute_WithOnlyTreeHash_ShouldListTreeInCorrectFormat()
    {
        // Arrange
        const string treeHash = "abc123";
        var args = new[] { treeHash };
        const string objectPath = "test/path";
        var decompressedBytes = "test bytes"u8.ToArray();
        var contentBytes = "test content"u8.ToArray();
        var gitObject = new GitObject(new ObjectHeader(ObjectType.Tree, contentBytes.Length), contentBytes);
        var tree = new Tree([
            new TreeEntry(TreeEntryMode.RegularFile, "someHash1", "file1.txt"),
            new TreeEntry(TreeEntryMode.ExecutableFile, "someHash2", "script.sh"),
            new TreeEntry(TreeEntryMode.SymbolicLink, "someHash3", "link.lnk"),
            new TreeEntry(TreeEntryMode.Directory, "someHash4", "src")
        ]);

        _objectLocator.GetGitObjectFilePath(_mockRepo, treeHash).Returns(objectPath);
        _compressionService.GetDecompressedObject(objectPath).Returns(decompressedBytes);
        _objectParser.ParseGitObject(decompressedBytes).Returns(gitObject);
        _objectParser.ParseTreeObject(gitObject.Content).Returns(tree);
        var lsTreeCommand = new LsTree(_repoFactory, _objectLocator, _compressionService, _objectParser, _outputWriter);

        // Act
        lsTreeCommand.Execute(args);

        // Assert
        foreach (var entry in tree.TreeEntries)
        {
            _outputWriter.Received(1)
                .WriteLine(
                    $"{entry.ModeValue.PadLeft(6, '0')} {entry.Type.ToString().ToLower()} {entry.Hash}     {entry.Name}");
        }
    }
}