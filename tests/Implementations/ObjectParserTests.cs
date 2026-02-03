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
        var expectedContentBytes = Encoding.ASCII.GetBytes(expectedContent);

        // Act
        var result = parser.ParseGitObject(gitObject);

        // Assert
        result.Header.Type.ShouldBe(expectedType);
        result.Header.Length.ShouldBe(expectedContent.Length);
        result.Content.ShouldBe(expectedContentBytes);
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

    [Theory]
    [InlineData("")]
    [InlineData("Hello World!")]
    [InlineData("Line1\nLine2\nLine3")]
    [InlineData("Special chars: !@#$%^&*()")]
    public void ParseBlobObject_WithValidContent_ShouldReturnBlobWithCorrectContent(string content)
    {
        // Arrange
        var contentBytes = Encoding.ASCII.GetBytes(content);
        var parser = new ObjectParser();

        // Act
        var result = parser.ParseBlobObject(contentBytes);

        // Assert
        result.AsciiContent.ShouldBe(content);
    }

    [Fact]
    public void ParseTreeObject_WithSingleEntry_ShouldReturnTreeWithOneEntry()
    {
        // Arrange
        var parser = new ObjectParser();
        byte[] sha20Byte = [132, 14, 51, 255, 10, 17, 34, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16];
        var expectedShaHex = Convert.ToHexStringLower(sha20Byte);
        var treeContentBytes = CreateTreeEntryBytes("100644", "file.txt", sha20Byte);

        // Act
        var result = parser.ParseTreeObject(treeContentBytes);

        // Assert
        result.TreeEntries.Count.ShouldBe(1);
        result.TreeEntries[0].Mode.ShouldBe(TreeEntryMode.RegularFile);
        result.TreeEntries[0].Name.ShouldBe("file.txt");
        result.TreeEntries[0].Hash.ShouldBe(expectedShaHex);
    }

    [Fact]
    public void ParseTreeObject_WithMultipleEntries_ShouldReturnTreeWithAllEntriesSortedByName()
    {
        // Arrange
        var parser = new ObjectParser();
        byte[] sha20Byte1 =
            [132, 14, 51, 255, 10, 17, 34, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16];
        byte[] sha20Byte2 = [10, 20, 30, 40, 50, 60, 70, 80, 90, 100, 110, 120, 130, 140, 150, 160, 170, 180, 190, 200];
        var expectedShaHex1 = Convert.ToHexStringLower(sha20Byte1);
        var expectedShaHex2 = Convert.ToHexStringLower(sha20Byte2);
        var entry1 = CreateTreeEntryBytes("40000", "subdir", sha20Byte1);
        var entry2 = CreateTreeEntryBytes("100644", "file.txt", sha20Byte2);
        byte[] treeContentBytes = [..entry1, ..entry2];

        // Act
        var result = parser.ParseTreeObject(treeContentBytes);

        // Assert
        result.TreeEntries.Count.ShouldBe(2);
        result.TreeEntries[0].Name.ShouldBe("file.txt");
        result.TreeEntries[0].Mode.ShouldBe(TreeEntryMode.RegularFile);
        result.TreeEntries[0].Hash.ShouldBe(expectedShaHex2);
        result.TreeEntries[1].Name.ShouldBe("subdir");
        result.TreeEntries[1].Mode.ShouldBe(TreeEntryMode.Directory);
        result.TreeEntries[1].Hash.ShouldBe(expectedShaHex1);
    }

    [Theory]
    [InlineData("100644", TreeEntryMode.RegularFile)]
    [InlineData("100755", TreeEntryMode.ExecutableFile)]
    [InlineData("120000", TreeEntryMode.SymbolicLink)]
    [InlineData("40000", TreeEntryMode.Directory)]
    public void ParseTreeObject_WithDifferentModes_ShouldReturnCorrectMode(string modeString,
        TreeEntryMode expectedMode)
    {
        // Arrange
        var parser = new ObjectParser();
        byte[] sha20Byte = [132, 14, 51, 255, 10, 17, 34, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16];
        var treeContentBytes = CreateTreeEntryBytes(modeString, "entry", sha20Byte);

        // Act
        var result = parser.ParseTreeObject(treeContentBytes);

        // Assert
        result.TreeEntries.Count.ShouldBe(1);
        result.TreeEntries[0].Mode.ShouldBe(expectedMode);
    }

    [Fact]
    public void ParseTreeObject_WithEmptyBytes_ShouldReturnEmptyTree()
    {
        // Arrange
        var parser = new ObjectParser();
        var treeContentBytes = Array.Empty<byte>();

        // Act
        var result = parser.ParseTreeObject(treeContentBytes);

        // Assert
        result.TreeEntries.ShouldBeEmpty();
    }

    [Fact]
    public void ParseTreeObject_WithUnsupportedMode_ShouldThrowArgumentException()
    {
        // Arrange
        var parser = new ObjectParser();
        byte[] sha20Byte = [132, 14, 51, 255, 10, 17, 34, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16];
        var treeContentBytes = CreateTreeEntryBytes("999999", "file.txt", sha20Byte);

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseTreeObject(treeContentBytes));
        exception.Message.ShouldContain("Unsupported tree entry type");
    }

    [Fact]
    public void ParseTreeObject_WithNoSpaceByte_ShouldThrowArgumentException()
    {
        // Arrange
        var parser = new ObjectParser();
        byte[] sha20Byte = [132, 14, 51, 255, 10, 17, 34, 68, 85, 102, 119, 136, 153, 170, 187, 204, 221, 238, 255, 16];
        var modeAndNameBytes = "100644file.txt"u8.ToArray();
        byte[] nullBytes = [Constants.NullByte];
        byte[] treeContentBytes = [..modeAndNameBytes, ..nullBytes, ..sha20Byte];

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseTreeObject(treeContentBytes));
        exception.Message.ShouldContain("no space byte");
    }

    [Fact]
    public void ParseTreeObject_WithNoNullByte_ShouldThrowArgumentException()
    {
        // Arrange
        var parser = new ObjectParser();
        var malformedBytes = "100644 file.txt"u8.ToArray();

        // Act & Assert
        var exception = Should.Throw<ArgumentException>(() => parser.ParseTreeObject(malformedBytes));
        exception.Message.ShouldContain("no null byte");
    }

    private static byte[] CreateGitObject(string type, string content)
    {
        // Format: <type> <length>\0<content>
        var header = $"{type} {content.Length}\0";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        var contentBytes = Encoding.ASCII.GetBytes(content);
        return [..headerBytes, ..contentBytes];
    }

    private static byte[] CreateTreeEntryBytes(string mode, string name, byte[] shaBytes)
    {
        // Format: <mode> <name>\0<20_byte_sha>
        var modeBytes = Encoding.ASCII.GetBytes(mode);
        byte[] spaceBytes = [Constants.SpaceByte];
        var nameBytes = Encoding.ASCII.GetBytes(name);
        byte[] nullBytes = [Constants.NullByte];

        return [..modeBytes, ..spaceBytes, ..nameBytes, ..nullBytes, ..shaBytes];
    }
}