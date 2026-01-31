using codecrafters_git.Commands;
using Shouldly;
using Xunit;

namespace codecrafters_git.tests.Commands;

public class InitTests : IDisposable
{
    private readonly StringWriter _consoleOutput;
    private readonly string _originalDirectory;
    private readonly string _tempRepoDirectory;

    public InitTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDirectory);
        Directory.SetCurrentDirectory(tempDirectory);

        // This resolves the symlink on macOS so adds the /private
        _tempRepoDirectory = Directory.GetCurrentDirectory();

        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);

        if (Directory.Exists(_tempRepoDirectory))
        {
            Directory.Delete(_tempRepoDirectory, true);
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Execute_WithoutArgs_ShouldCreateRepo()
    {
        // Arrange
        var initCommand = new Init();

        // Act
        initCommand.Execute([]);

        // Assert
        var gitDirectory = Path.Combine(_tempRepoDirectory, ".git/");
        Directory.Exists(gitDirectory).ShouldBeTrue();
        Directory.Exists(Path.Combine(gitDirectory, "objects")).ShouldBeTrue();
        Directory.Exists(Path.Combine(gitDirectory, "refs")).ShouldBeTrue();
        File.Exists(Path.Combine(gitDirectory, "HEAD")).ShouldBeTrue();
        File.ReadAllText(Path.Combine(gitDirectory, "HEAD")).ShouldBe("ref: refs/heads/main\n");

        var output = _consoleOutput.ToString();
        output.ShouldContain("Initialized empty Git repository in");
        output.ShouldContain(gitDirectory);
    }

    [Fact]
    public void Execute_WithArgs_ShouldCreateRepoInSpecificDirectory()
    {
        // Arrange
        var initCommand = new Init();
        const string specificDirectory = "test";

        // Act
        initCommand.Execute([specificDirectory]);

        // Assert
        var gitDirectory = Path.Combine(_tempRepoDirectory, specificDirectory, ".git");
        Directory.Exists(gitDirectory).ShouldBeTrue();
        Directory.Exists(Path.Combine(gitDirectory, "objects")).ShouldBeTrue();
        Directory.Exists(Path.Combine(gitDirectory, "refs")).ShouldBeTrue();
        File.Exists(Path.Combine(gitDirectory, "HEAD")).ShouldBeTrue();
        File.ReadAllText(Path.Combine(gitDirectory, "HEAD")).ShouldBe("ref: refs/heads/main\n");

        var output = _consoleOutput.ToString();
        output.ShouldContain("Initialized empty Git repository in");
        output.ShouldContain(gitDirectory);
    }
}