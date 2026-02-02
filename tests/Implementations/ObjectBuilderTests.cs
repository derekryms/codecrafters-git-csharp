using System.Text;
using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class ObjectBuilderTests
{
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
        var objectBuilder = new ObjectBuilder();

        // Act
        var result = objectBuilder.BuildGitObject(type, contentBytes);

        // Assert
        result.ShouldBe(expectedBytes);
    }
}