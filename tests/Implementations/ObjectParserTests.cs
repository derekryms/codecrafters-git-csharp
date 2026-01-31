using System.Text;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ObjectParserTests
{
    [Theory]
    [InlineData("blob", ObjectType.Blob, "")]
    [InlineData("blob", ObjectType.Blob, "Hello World!")]
    [InlineData("tree", ObjectType.Tree, "blob 87fas76f986")]
    [InlineData("commit", ObjectType.Commit, "tree 87fas76f986\nauthor John Doe")]
    [InlineData("tag", ObjectType.Tag, "tag v1.0\nobject 87fas76f986")]
    public void ParseGitObject_WithDifferentObjectTypes_ShouldReturnCorrectObjectType(string typeString,
        ObjectType expectedType, string expectedContent)
    {
        // Arrange
        var gitObject = CreateGitObject(typeString, expectedContent);
        var parser = new ObjectParser();

        // Act
        var (type, parsedContent) = parser.ParseGitObject(gitObject);

        // Assert
        type.ShouldBe(expectedType);
        parsedContent.ShouldBe(expectedContent);
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
        // Arrange
        var invalidObject = "blob12\0content"u8.ToArray();
        var parser = new ObjectParser();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseGitObject(invalidObject));
        exception.Message.ShouldContain("no space byte found");
    }

    private static byte[] CreateGitObject(string type, string content)
    {
        var header = $"{type} {content.Length}\0";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        var contentBytes = Encoding.ASCII.GetBytes(content);
        return [..headerBytes, ..contentBytes];
    }
}