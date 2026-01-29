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
        await new Init("").Run([]);
        break;
    case "cat-file":
        await new CatFile("").Run(otherArgs);
        break;
    case "hash-object":
        await new HashObject("").Run(otherArgs);
        break;
    case "ls-tree":
        await new LsTree("").Run(otherArgs);
        break;
    case "write-tree":
        await new WriteTree("").Run(otherArgs);
        break;
    case "commit-tree":
        await new CommitTree("").Run(otherArgs);
        break;
    case "clone":
        await new Clone().Run(otherArgs);
        break;
    default:
        throw new ArgumentException($"Unknown command {command}");
}