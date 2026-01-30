using codecrafters_git.Commands;
using Shouldly;
using Xunit;
using System.IO;

namespace codecrafters_git.tests;

public class InitTests : IDisposable
{
    private readonly string _tempDirectory;
    private readonly string _originalDirectory;
    private readonly StringWriter _consoleOutput;

    public InitTests()
    {
        _originalDirectory = Directory.GetCurrentDirectory();
        _tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
        Directory.SetCurrentDirectory(_tempDirectory);
        
        _consoleOutput = new StringWriter();
        Console.SetOut(_consoleOutput);
    }

    public void Dispose()
    {
        Directory.SetCurrentDirectory(_originalDirectory);
        if (Directory.Exists(_tempDirectory))
        {
            Directory.Delete(_tempDirectory, recursive: true);
        }
        
        _consoleOutput.Flush();
        
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void Init_Execute_WithoutDirectory_ShouldCreateNewRepo()
    {
        // Arrange
        var initCommand = new Init();
        
        // Act
        initCommand.Execute([]);
        
        // Assert
        var gitDirectory = Path.Combine(_tempDirectory, ".git/");
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
    public void Init_Execute_WithDirectory_ShouldCreateRepoInSpecifiedDirectory()
    {
        // Arrange
        var initCommand = new Init();
        var subDirectory = "test-repo";
        
        // Act
        initCommand.Execute([subDirectory]);
        
        // Assert
        var gitDirectory = Path.Combine(_tempDirectory, subDirectory, ".git/");
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