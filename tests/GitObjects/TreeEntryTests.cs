using codecrafters_git.GitObjects;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.GitObjects;

public class TreeEntryTests
{
    [Theory]
    [InlineData(TreeEntryMode.RegularFile, "100644")]
    [InlineData(TreeEntryMode.ExecutableFile, "100755")]
    [InlineData(TreeEntryMode.SymbolicLink, "120000")]
    [InlineData(TreeEntryMode.Directory, "40000")]
    public void TreeEntry_ModeValue_ShouldReturnCorrectString(TreeEntryMode mode, string expectedModeValue)
    {
        // Arrange
        var treeEntry = new TreeEntry(mode, "someHash", "someName");

        // Act
        var modeValue = treeEntry.ModeValue;

        // Assert
        modeValue.ShouldBe(expectedModeValue);
    }

    [Fact]
    public void TreeEntry_ModeValue_ShouldThrowArgumentOutOfRangeException_ForInvalidMode()
    {
        // Arrange
        const TreeEntryMode invalidMode = (TreeEntryMode)999;
        var treeEntry = new TreeEntry(invalidMode, "someHash", "someName");

        // Act & Assert
        Should.Throw<ArgumentOutOfRangeException>(() => { _ = treeEntry.ModeValue; });
    }
}