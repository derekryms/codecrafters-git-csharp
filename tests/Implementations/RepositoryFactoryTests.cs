using codecrafters_git.Abstractions;
using codecrafters_git.Implementations;
using NSubstitute;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class RepositoryFactoryTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();

    [Fact]
    public void CreateAtCurrentDirectory_ShouldSetCorrectPaths()
    {
        // Arrange
        const string repoDirectory = "/test/repo";
        _fileSystem.GetCurrentDirectory().Returns(repoDirectory);
        var repoFactory = new RepositoryFactory(_fileSystem);

        // Act
        var repo = repoFactory.CreateAtCurrentDirectory();

        // Assert
        repo.GitDirectory.ShouldBe(Path.Combine(repoDirectory, ".git"));
        repo.ObjectsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "objects"));
        repo.RefsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "refs"));
        repo.HeadFile.ShouldBe(Path.Combine(repo.GitDirectory, "HEAD"));
    }

    [Fact]
    public void CreateAtSpecificDirectory_ShouldSetCorrectPaths()
    {
        // Arrange
        const string specificDirectory = "test";
        const string currentDirectory = "/test/repo";
        _fileSystem.GetCurrentDirectory().Returns(currentDirectory);
        var repoFactory = new RepositoryFactory(_fileSystem);
        var repoDirectory = Path.Combine(currentDirectory, specificDirectory);

        // Act
        var repo = repoFactory.CreateAtSpecificDirectory(specificDirectory);

        // Assert
        repo.GitDirectory.ShouldBe(Path.Combine(repoDirectory, ".git"));
        repo.ObjectsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "objects"));
        repo.RefsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "refs"));
        repo.HeadFile.ShouldBe(Path.Combine(repo.GitDirectory, "HEAD"));
    }
}