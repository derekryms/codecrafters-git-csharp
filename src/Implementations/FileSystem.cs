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

    public Stream OpenRead(string path)
    {
        return File.OpenRead(path);
    }

    public Stream OpenWrite(string path)
    {
        return File.OpenWrite(path);
    }

    public byte[] ReadAllBytes(string file)
    {
        return File.ReadAllBytes(file);
    }

    public IEnumerable<string> GetFiles(string directory)
    {
        return Directory.GetFiles(directory);
    }

    public IEnumerable<string> GetDirectories(string directory)
    {
        return Directory.GetDirectories(directory);
    }

    public string GetFileName(string file)
    {
        return Path.GetFileName(file);
    }
}