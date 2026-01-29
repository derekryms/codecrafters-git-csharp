namespace codecrafters_git.Commands;

public class Init(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        Directory.CreateDirectory($"{rootDirectory}.git");
        Directory.CreateDirectory($"{rootDirectory}.git/objects");
        Directory.CreateDirectory($"{rootDirectory}.git/refs");
        File.WriteAllText($"{rootDirectory}.git/HEAD", "ref: refs/heads/main\n");
        Console.WriteLine("Initialized git directory");
        
        return Task.CompletedTask;
    }
}