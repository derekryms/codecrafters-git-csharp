using Shouldly;
using Xunit;

namespace codecrafters_git.tests;

public class RepositoryTests
{
    [Fact]
    public void CreateAtCurrentDirectory_ShouldSetCorrectPaths()
    {
        // Arrange
        var repoDirectory = Directory.GetCurrentDirectory();

        // Act
        var repo = Repository.CreateAtCurrentDirectory();

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

        // Act
        var repo = Repository.CreateAtSpecificDirectory(specificDirectory);

        // Assert
        repo.GitDirectory.ShouldBe(Path.Combine(repoDirectory, ".git"));
        repo.ObjectsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "objects"));
        repo.RefsDirectory.ShouldBe(Path.Combine(repo.GitDirectory, "refs"));
        repo.HeadPath.ShouldBe(Path.Combine(repo.GitDirectory, "HEAD"));
    }
}