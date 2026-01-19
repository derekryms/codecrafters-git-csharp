using codecrafters_git.Commands;

if (args.Length < 1)
{
    Console.WriteLine("Please provide a command.");
    return;
}

var command = args[0];
var otherArgs = args[1..];

switch (command)
{
    case "init":
        new Init().Run([]);
        break;
    case "cat-file":
        new CatFile().Run(otherArgs);
        break;
    case "hash-object":
        new HashObject().Run(otherArgs);
        break;
    case "ls-tree":
        new LsTree().Run(otherArgs);
        break;
    case "write-tree":
        new WriteTree().Run(otherArgs);
        break;
    case "commit-tree":
        new CommitTree().Run(otherArgs);
        break;
    case "clone":
        new Clone().Run(otherArgs);
        break;
    default:
        throw new ArgumentException($"Unknown command {command}");
}