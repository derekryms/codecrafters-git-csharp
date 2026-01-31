namespace codecrafters_git;

public interface IFileSystem
{
    void CreateDirectory(string path);
    void WriteAllText(string path, string contents);
    bool DirectoryExists(string path);
    bool FileExists(string path);
}