using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class HashObject : ICommand
{
    public void Run(string[] args)
    {
        var option = args[0];
        var fileName = args[1];

        if (option == "-w")
        {
            var blob = new GitBlob(File.ReadAllText(fileName));
            var hash = Helpers.Compress(blob.UncompressedDataBytes);
            Console.Write(hash);
        }
    }
}