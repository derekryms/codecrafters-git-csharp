using System.Text;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ObjectParserTests
{
    [Fact]
    public void ParseGitObject_WithBlobObject_ShouldReturnBlobTypeAndContent()
    {
        // Arrange
        const string content = "Hello World!";
        var gitObject = CreateGitObject("blob", content);
        var parser = new ObjectParser();

        // Act
        var (type, parsedContent) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(ObjectType.Blob);
        parsedContent.ShouldBe(content);
    }

    [Fact]
    public void ParseGitObject_WithTreeObject_ShouldReturnTreeType()
    {
        // Arrange
        const string content = "tree content here";
        var gitObject = CreateGitObject("tree", content);
        var parser = new ObjectParser();

        // Act
        var (type, _) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(ObjectType.Tree);
    }

    [Fact]
    public void ParseGitObject_WithCommitObject_ShouldReturnCommitType()
    {
        // Arrange
        const string content = "commit content here";
        var gitObject = CreateGitObject("commit", content);
        var parser = new ObjectParser();

        // Act
        var (type, _) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(ObjectType.Commit);
    }

    [Fact]
    public void ParseGitObject_WithTagObject_ShouldReturnTagType()
    {
        // Arrange
        const string content = "tag content here";
        var gitObject = CreateGitObject("tag", content);
        var parser = new ObjectParser();

        // Act
        var (type, _) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(ObjectType.Tag);
    }

    [Fact]
    public void ParseGitObject_WithEmptyContent_ShouldReturnEmptyString()
    {
        // Arrange
        var gitObject = CreateGitObject("blob", "");
        var parser = new ObjectParser();

        // Act
        var (type, content) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(ObjectType.Blob);
        content.ShouldBeEmpty();
    }

    [Fact]
    public void ParseGitObject_WithNoNullByte_ShouldThrowArgumentException()
    {
        // Arrange
        var invalidObject = "blob 12 no null byte"u8.ToArray();
        var parser = new ObjectParser();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseGitObject(invalidObject));
        exception.Message.ShouldContain("no null byte");
    }

    [Fact]
    public void ParseGitObject_WithUnsupportedType_ShouldThrowArgumentException()
    {
        // Arrange
        var gitObject = CreateGitObject("unsupported", "content");
        var parser = new ObjectParser();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseGitObject(gitObject));
        exception.Message.ShouldContain("Unsupported git object type");
    }

    [Fact]
    public void ParseGitObject_WithInvalidHeader_ShouldThrowArgumentException()
    {
        // Arrange - header without space byte (e.g., "blob12\0content")
        var invalidObject = "blob12\0content"u8.ToArray();
        var parser = new ObjectParser();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseGitObject(invalidObject));
        exception.Message.ShouldContain("no space byte found");
    }

    [Fact]
    public void ParseGitObject_WithContentContainingNullBytes_ShouldPreserveContent()
    {
        // Arrange - content with special characters
        const string content = "line1\nline2\ttab";
        var gitObject = CreateGitObject("blob", content);
        var parser = new ObjectParser();

        // Act
        var (_, parsedContent) = parser.ParseGitObject(gitObject);

        // Assert
        parsedContent.ShouldBe(content);
    }

    private static byte[] CreateGitObject(string type, string content)
    {
        var header = $"{type} {content.Length}\0";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        var contentBytes = Encoding.ASCII.GetBytes(content);

        var result = new byte[headerBytes.Length + contentBytes.Length];
        headerBytes.CopyTo(result, 0);
        contentBytes.CopyTo(result, headerBytes.Length);

        return result;
    }
}