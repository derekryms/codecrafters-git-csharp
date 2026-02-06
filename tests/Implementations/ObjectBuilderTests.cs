using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using NSubstitute;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ObjectBuilderTests
{
    private readonly IFileSystem _fileSystem = Substitute.For<IFileSystem>();

    [Theory]
    [InlineData(ObjectType.Blob, "Hello, World!")]
    [InlineData(ObjectType.Tree, "blob hash fileName.ext")]
    [InlineData(ObjectType.Commit, "tree hash\npatent hash\n")]
    [InlineData(ObjectType.Tag, "object hash\ntype commit\ntag v1.0")]
    public void BuildGitObject_ShouldBuildCorrectGitObject(ObjectType type, string content)
    {
        // Arrange
        var contentBytes = Encoding.ASCII.GetBytes(content);
        var expectedString = $"{type.ToString().ToLower()} {content.Length}\0{content}";
        var expectedBytes = Encoding.ASCII.GetBytes(expectedString);
        var objectBuilder = new ObjectBuilder(_fileSystem);

        // Act
        var result = objectBuilder.BuildGitObject(type, contentBytes);

        // Assert
        result.ShouldBe(expectedBytes);
    }

    [Fact]
    public void BuildTreeObjectContentBytes_WithMixedFilesAndRecursiveDirectories_ShouldBuildCorrectTree()
    {
        // Arrange
        const string rootDir = "/test";
        const string srcDirName = "src";
        const string libDirName = "lib";
        const string utilsDirName = "utils";
        const string coreDirName = "core";
        const string readmeFileName = "readme.txt";
        const string appFileName = "app.txt";
        const string mainFileName = "main.txt";
        const string helperFileName = "helper.txt";
        const string baseFileName = "base.txt";

        var srcDir = $"{rootDir}/{srcDirName}";
        var libDir = $"{rootDir}/{libDirName}";
        var srcSubDir = $"{srcDir}/{utilsDirName}";
        var libSubDir = $"{libDir}/{coreDirName}";
        var readmeFilePath = $"{rootDir}/{readmeFileName}";
        var appFilePath = $"{rootDir}/{appFileName}";
        var mainFilePath = $"{srcDir}/{mainFileName}";
        var helperFilePath = $"{srcSubDir}/{helperFileName}";
        var baseFilePath = $"{libSubDir}/{baseFileName}";

        var fileContent = "content"u8.ToArray();

        // Root level: files and directories
        _fileSystem.GetFiles(rootDir).Returns([readmeFilePath, appFilePath]);
        _fileSystem.GetDirectories(rootDir).Returns([srcDir, libDir]);
        _fileSystem.GetFileName(readmeFilePath).Returns(readmeFileName);
        _fileSystem.GetFileName(appFilePath).Returns(appFileName);
        _fileSystem.GetFileName(srcDir).Returns(srcDirName);
        _fileSystem.GetFileName(libDir).Returns(libDirName);
        _fileSystem.ReadAllBytes(readmeFilePath).Returns(fileContent);
        _fileSystem.ReadAllBytes(appFilePath).Returns(fileContent);

        // src directory with subdirectory
        _fileSystem.GetFiles(srcDir).Returns([mainFilePath]);
        _fileSystem.GetDirectories(srcDir).Returns([srcSubDir]);
        _fileSystem.GetFileName(mainFilePath).Returns(mainFileName);
        _fileSystem.GetFileName(srcSubDir).Returns(utilsDirName);
        _fileSystem.ReadAllBytes(mainFilePath).Returns(fileContent);

        // src/utils subdirectory
        _fileSystem.GetFiles(srcSubDir).Returns([helperFilePath]);
        _fileSystem.GetDirectories(srcSubDir).Returns([]);
        _fileSystem.GetFileName(helperFilePath).Returns(helperFileName);
        _fileSystem.ReadAllBytes(helperFilePath).Returns(fileContent);

        // lib directory with subdirectory
        _fileSystem.GetFiles(libDir).Returns([]);
        _fileSystem.GetDirectories(libDir).Returns([libSubDir]);
        _fileSystem.GetFileName(libSubDir).Returns(coreDirName);

        // lib/core subdirectory
        _fileSystem.GetFiles(libSubDir).Returns([baseFilePath]);
        _fileSystem.GetDirectories(libSubDir).Returns([]);
        _fileSystem.GetFileName(baseFilePath).Returns(baseFileName);
        _fileSystem.ReadAllBytes(baseFilePath).Returns(fileContent);

        var objectBuilder = new ObjectBuilder(_fileSystem);

        // Act
        var result = objectBuilder.BuildTreeObjectContentBytes(rootDir);

        // Assert - root should contain sorted entries: app.txt, lib, readme.txt, src
        var resultString = Encoding.ASCII.GetString(result);
        var appIndex = resultString.IndexOf(appFileName, StringComparison.Ordinal);
        var libIndex = resultString.IndexOf(libDirName, StringComparison.Ordinal);
        var readmeIndex = resultString.IndexOf(readmeFileName, StringComparison.Ordinal);
        var srcIndex = resultString.IndexOf(srcDirName, StringComparison.Ordinal);

        appIndex.ShouldBeLessThan(libIndex);
        libIndex.ShouldBeLessThan(readmeIndex);
        readmeIndex.ShouldBeLessThan(srcIndex);

        // Verify tree entries have mode 40000
        resultString.ShouldContain($"40000 {libDirName}");
        resultString.ShouldContain($"40000 {srcDirName}");

        // Verify file entries have mode 100644
        resultString.ShouldContain($"100644 {appFileName}");
        resultString.ShouldContain($"100644 {readmeFileName}");

        // Verify nested directories were processed recursively
        _fileSystem.Received().GetFiles(srcDir);
        _fileSystem.Received().GetFiles(srcSubDir);
        _fileSystem.Received().GetFiles(libDir);
        _fileSystem.Received().GetFiles(libSubDir);
    }

    [Fact]
    public void BuildTreeObjectContentBytes_ShouldExcludeGitDirectory()
    {
        // Arrange
        const string rootDir = "/test";
        const string gitDirName = ".git";
        const string srcDirName = "src";
        const string fileName = "file.txt";

        var gitDir = $"{rootDir}/{gitDirName}";
        var srcDir = $"{rootDir}/{srcDirName}";
        var filePath = $"{srcDir}/{fileName}";

        _fileSystem.GetFiles(rootDir).Returns([]);
        _fileSystem.GetDirectories(rootDir).Returns([gitDir, srcDir]);
        _fileSystem.GetFileName(gitDir).Returns(gitDirName);
        _fileSystem.GetFileName(srcDir).Returns(srcDirName);

        // Setup src subdirectory
        _fileSystem.GetFiles(srcDir).Returns([filePath]);
        _fileSystem.GetDirectories(srcDir).Returns([]);
        _fileSystem.GetFileName(filePath).Returns(fileName);
        _fileSystem.ReadAllBytes(filePath).Returns("content"u8.ToArray());

        var objectBuilder = new ObjectBuilder(_fileSystem);

        // Act
        var result = objectBuilder.BuildTreeObjectContentBytes(rootDir);

        // Assert - should not contain .git directory
        var resultString = Encoding.ASCII.GetString(result);
        resultString.ShouldNotContain(gitDirName);
        resultString.ShouldContain(srcDirName);
    }

    [Fact]
    public void BuildCommitObjectContentBytes_ShouldReturnCorrectlyFormattedCommit()
    {
        // Arrange
        const string treeHash = "abc123def456789012345678901234567890abcd";
        const string parentHash = "def456789012345678901234567890abcdef1234";
        const string message = "Initial commit";
        var objectBuilder = new ObjectBuilder(_fileSystem);

        // Act
        var result = objectBuilder.BuildCommitObjectContentBytes(treeHash, parentHash, message);
        var resultString = Encoding.ASCII.GetString(result);

        // Assert
        resultString.ShouldContain($"tree {treeHash}\n");
        resultString.ShouldContain($"parent {parentHash}\n");
        resultString.ShouldContain("author Derek Ryms <dryms@arielcorp.com>");
        resultString.ShouldEndWith($"\n{message}\n");
    }
}