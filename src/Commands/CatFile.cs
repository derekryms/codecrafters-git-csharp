using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class CatFile(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        var option = args[0];
        var hash = args[1];

        if (option == "-p")
        {
            var contents = GitBlob.GetContent(Helpers.GetDecompressedBytes(rootDirectory, hash));
            Console.Write(contents);
        }
        
        return Task.CompletedTask;
    }
}