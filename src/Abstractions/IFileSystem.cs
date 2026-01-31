namespace codecrafters_git.Abstractions;

public interface IFileSystem
{
    void CreateDirectory(string path);
    void WriteAllText(string path, string contents);
    bool DirectoryExists(string path);
    bool FileExists(string path);
}