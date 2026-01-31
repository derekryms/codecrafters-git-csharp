using codecrafters_git.Implementations;
using NSubstitute;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Implementations;

public class CommandResolverTests
{
    [Fact]
    public void Resolve_WithUnknownCommand_ShouldReturnNull()
    {
        // Arrange
        var serviceProvider = Substitute.For<IServiceProvider>();
        var resolver = new CommandResolver(serviceProvider);

        // Act
        var result = resolver.Resolve("unknown-command");

        // Assert
        result.ShouldBeNull();
    }
}