using codecrafters_git.Abstractions;

namespace codecrafters_git.Implementations;

public class FileSystem : IFileSystem
{
    public string GetCurrentDirectory()
    {
        return Directory.GetCurrentDirectory();
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public void WriteAllText(string path, string contents)
    {
        File.WriteAllText(path, contents);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }
}