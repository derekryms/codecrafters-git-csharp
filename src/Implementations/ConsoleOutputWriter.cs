using codecrafters_git.Abstractions;

namespace codecrafters_git.Implementations;

public class ConsoleOutputWriter : IOutputWriter
{
    public void Write(string value)
    {
        Console.Write(value);
    }

    public void WriteLine(string value)
    {
        Console.WriteLine(value);
    }
}