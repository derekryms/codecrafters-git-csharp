using codecrafters_git.Abstractions;
using codecrafters_git.Commands;
using codecrafters_git.GitObjects;
using codecrafters_git.tests.TestHelpers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class InitTests
{
    private readonly IFileSystem _fileSystem;
    private readonly ILogger<Init> _logger;
    private readonly Repository _mockRepo;
    private readonly IRepositoryFactory _repoFactory;

    public InitTests()
    {
        _fileSystem = Substitute.For<IFileSystem>();
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _logger = Substitute.For<ILogger<Init>>();
        _repoFactory = Substitute.For<IRepositoryFactory>();
        _mockRepo = new Repository("/test/repo");
    }

    [Fact]
    public void Execute_WithoutArgs_ShouldCreateRepo()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _logger);
        _repoFactory.CreateAtCurrentDirectory().Returns(_mockRepo);

        // Act
        initCommand.Execute([]);

        // Assert
        _repoFactory.Received(1).CreateAtCurrentDirectory();
        _fileSystem.Received(1).CreateDirectory(_mockRepo.GitDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.ObjectsDirectory);
        _fileSystem.Received(1).CreateDirectory(_mockRepo.RefsDirectory);
        _fileSystem.Received(1).WriteAllText(_mockRepo.HeadFile, "ref: refs/heads/main\n");
        _logger.ReceivedLogContaining(LogLevel.Information, "Initialized empty Git repository", _mockRepo.GitDirectory);
    }

    [Fact]
    public void Execute_WithArgs_ShouldCreateRepoInSpecificDirectory()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _logger);
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
        _logger.ReceivedLogContaining(LogLevel.Information, "Initialized empty Git repository", _mockRepo.GitDirectory);
    }

    [Fact]
    public void Execute_WithTooManyArgs_ShouldLogUsageWarning()
    {
        // Arrange
        var initCommand = new Init(_repoFactory, _fileSystem, _logger);

        // Act
        initCommand.Execute(["arg1", "arg2"]);

        // Assert
        _repoFactory.DidNotReceive().CreateAtCurrentDirectory();
        _repoFactory.DidNotReceive().CreateAtSpecificDirectory(Arg.Any<string>());
        _fileSystem.DidNotReceive().CreateDirectory(Arg.Any<string>());
        _fileSystem.DidNotReceive().WriteAllText(Arg.Any<string>(), Arg.Any<string>());
        _logger.ReceivedLogContaining(LogLevel.Warning, "Usage: init [directory]");
    }
}