using Shouldly;
using Xunit;

namespace codecrafters_git.tests;

public class RepositoryFactoryTests
{
    [Fact]
    public void CreateAtCurrentDirectory_ShouldSetCorrectPaths()
    {
        // Arrange
        var repoDirectory = Directory.GetCurrentDirectory();
        var repoFactory = new RepositoryFactory();

        // Act
        var repo = repoFactory.CreateAtCurrentDirectory();

        // Assert
        repo.GitDirectory.ShouldBe(Path.Combine(repoDirectory, ".git"));
        repo.ObjectsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "objects"));
        repo.RefsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "refs"));
        repo.HeadPath.ShouldBe(Path.Combine(repo.GitDirectory, "HEAD"));
    }

    [Fact]
    public void CreateAtSpecificDirectory_ShouldSetCorrectPaths()
    {
        // Arrange
        const string specificDirectory = "test";
        var repoDirectory = Path.Combine(Directory.GetCurrentDirectory(), specificDirectory);
        var repoFactory = new RepositoryFactory();

        // Act
        var repo = repoFactory.CreateAtSpecificDirectory(specificDirectory);

        // Assert
        repo.GitDirectory.ShouldBe(Path.Combine(repoDirectory, ".git"));
        repo.ObjectsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "objects"));
        repo.RefsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "refs"));
        repo.HeadPath.ShouldBe(Path.Combine(repo.GitDirectory, "HEAD"));
    }
}