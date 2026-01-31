using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IRepositoryFactory
{
    Repository CreateAtSpecificDirectory(string specificDirectory);
    Repository CreateAtCurrentDirectory();
}