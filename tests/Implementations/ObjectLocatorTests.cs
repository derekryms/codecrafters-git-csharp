using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using NSubstitute;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ObjectLocatorTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();
    private readonly Repository _repo = new("/test/repo");

    [Fact]
    public void GetGitObjectFilePath_WhenObjectExists_ShouldReturnCorrectPath()
    {
        // Arrange - full 40 character SHA-1 hash
        const string objectHash = "e69de29bb2d1d6434b8b29ae775ad8c2e48c5391";
        var expectedPath = Path.Combine(_repo.ObjectsDirectory, "e6", "9de29bb2d1d6434b8b29ae775ad8c2e48c5391");
        _fileSystem.FileExists(expectedPath).Returns(true);
        var locator = new ObjectLocator(_fileSystem);

        // Act
        var result = locator.GetGitObjectFilePath(_repo, objectHash);

        // Assert
        result.ShouldBe(expectedPath);
    }

    [Fact]
    public void GetGitObjectFilePath_WhenObjectDoesNotExist_ShouldThrowFileNotFoundException()
    {
        // Arrange
        const string objectHash = "abc123def456";
        var expectedPath = Path.Combine(_repo.ObjectsDirectory, "ab", "c123def456");
        _fileSystem.FileExists(expectedPath).Returns(false);
        var locator = new ObjectLocator(_fileSystem);

        // Act & Assert
        var exception = Should.Throw<FileNotFoundException>(() =>
            locator.GetGitObjectFilePath(_repo, objectHash));
        exception.Message.ShouldContain(objectHash);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(true, 0)]
    public void CreateGitObjectDirectory_ShouldReturnFullPath_AndShouldOnlyCreateDirectoryIfNotExists(
        bool directoryExists, int directoryCreateCalls)
    {
        // Arrange
        const string objectHash = "abc123def456";
        var expectedDirPath = Path.Combine(_repo.ObjectsDirectory, "ab");
        var expectedFilePath = Path.Combine(expectedDirPath, "c123def456");
        _fileSystem.DirectoryExists(expectedDirPath).Returns(directoryExists);
        var locator = new ObjectLocator(_fileSystem);

        // Act
        var result = locator.CreateGitObjectDirectory(_repo, objectHash);

        // Assert
        _fileSystem.Received(directoryCreateCalls).CreateDirectory(expectedDirPath);
        result.ShouldBe(expectedFilePath);
    }
}