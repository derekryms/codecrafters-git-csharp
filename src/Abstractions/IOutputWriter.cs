namespace codecrafters_git.Abstractions;

public interface IOutputWriter
{
    void Write(string value);
    void WriteLine(string value);
}