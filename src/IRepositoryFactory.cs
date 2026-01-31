namespace codecrafters_git;

public interface IRepositoryFactory
{
    Repository CreateAtSpecificDirectory(string specificDirectory);
    Repository CreateAtCurrentDirectory();
}