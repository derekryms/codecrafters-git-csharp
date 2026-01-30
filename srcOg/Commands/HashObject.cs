using codecrafters_git.srcOg.GitObjects;

namespace codecrafters_git.srcOg.Commands;

public class HashObject(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        var option = args[0];
        var fileName = args[1];

        if (option == "-w")
        {
            var blob = new GitBlob(File.ReadAllText(fileName));
            var hash = Helpers.Compress(rootDirectory, blob.UncompressedDataBytes);
            Console.Write(hash);
        }
        
        return Task.CompletedTask;
    }
}