using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class RepositoryFactory(IFileSystem fileSystem) : IRepositoryFactory
{
    public Repository CreateAtSpecificDirectory(string specificDirectory)
    {
        return new Repository(Path.Combine(fileSystem.GetCurrentDirectory(), specificDirectory));
    }

    public Repository CreateAtCurrentDirectory()
    {
        return CreateAtSpecificDirectory(fileSystem.GetCurrentDirectory());
    }
}