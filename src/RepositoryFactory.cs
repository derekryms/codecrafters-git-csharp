namespace codecrafters_git;

public class RepositoryFactory
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