using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class RepositoryFactory : IRepositoryFactory
{
    public Repository CreateAtSpecificDirectory(string specificDirectory)
    {
        return new Repository(Path.Combine(Directory.GetCurrentDirectory(), specificDirectory));
    }

    public Repository CreateAtCurrentDirectory()
    {
        return CreateAtSpecificDirectory(Directory.GetCurrentDirectory());
    }
}