using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class InitTests
{
    private readonly IFileSystem _fileSystem;
    private readonly IOutputWriter _outputWriter;
    private readonly Repository _mockRepo;
    private readonly IRepositoryFactory _repoFactory;

    public InitTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _outputWriter = Substitute.For<IOutputWriter>();
        _mockRepo = new Repository("/test/repo");
    }

    [Fact]
    public void Execute_WithoutArgs_ShouldCreateRepo()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _outputWriter);
        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);

        // Act
        initCommand.Execute([]);

        // Assert
        _repoFactory.Received(1).CreateAtCurrentDirectory();
        _fileSystem.Received(1).CreateDirectory(_mockRepo.GitDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.ObjectsDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.RefsDirectory);
        _fileSystem.Received(1).WriteAllText(_mockRepo.HeadFile, "ref: refs/heads/main\n");
        _outputWriter.Received(1).WriteLine($"Initialized empty Git repository in {_mockRepo.GitDirectory}/");
    }

    [Fact]
    public void Execute_WithArgs_ShouldCreateRepoInSpecificDirectory()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _outputWriter);
        const string specificDirectory = "test";
        _repoFactory.CreateAtSpecificDirectory(specificDirectory).Returns(_mockRepo);

        // Act
        initCommand.Execute([specificDirectory]);

        // Assert
        _repoFactory.Received(1).CreateAtSpecificDirectory(specificDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.GitDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.ObjectsDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.RefsDirectory);
        _fileSystem.Received(1).WriteAllText(_mockRepo.HeadFile, "ref: refs/heads/main\n");
        _outputWriter.Received(1).WriteLine($"Initialized empty Git repository in {_mockRepo.GitDirectory}/");
    }

    [Fact]
    public void Execute_WithTooManyArgs_ShouldWriteUsageMessage()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _outputWriter);

        // Act
        initCommand.Execute(["arg1", "arg2"]);

        // Assert
        _repoFactory.DidNotReceive().CreateAtCurrentDirectory();
        _repoFactory.DidNotReceive().CreateAtSpecificDirectory(Arg.Any<string>());
        _fileSystem.DidNotReceive().CreateDirectory(Arg.Any<string>());
        _fileSystem.DidNotReceive().WriteAllText(Arg.Any<string>(), Arg.Any<string>());
        _outputWriter.Received(1).WriteLine("Usage: init [directory]");
    }
}