using codecrafters_git;
using codecrafters_git.Commands;

if (args.Length < 1)
{
    throw new ArgumentException("No arguments have been provided.");
}

var repoFactory = new RepositoryFactory();
var commandMap = new Dictionary<string, ICommand>
{
    { "init", new Init(repoFactory) },
    { "cat-file", new CatFile(repoFactory.CreateAtCurrentDirectory()) }
};

var command = args[0];
if (!commandMap.TryGetValue(command, out var value))
{
    throw new ArgumentException($"Unknown command: {command}.");
}

var remainingArgs = args[1..];
value.Execute(remainingArgs);