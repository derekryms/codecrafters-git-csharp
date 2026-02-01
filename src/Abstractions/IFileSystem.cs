namespace codecrafters_git.Abstractions;

public interface IFileSystem
{
    string GetCurrentDirectory();
    void CreateDirectory(string path);
    void WriteAllText(string path, string contents);
    bool DirectoryExists(string path);
    bool FileExists(string path);
    Stream OpenRead(string path);
    Stream OpenWrite(string path);
    byte[] ReadAllBytes(string file);
}