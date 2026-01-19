using System.IO.Compression;
using System.Text;
using codecrafters_git;

if (args.Length < 1)
{
    Console.WriteLine("Please provide a command.");
    return;
}

var command = args[0];

switch (command)
{
    case "init":
        Directory.CreateDirectory(".git");
        Directory.CreateDirectory(".git/objects");
        Directory.CreateDirectory(".git/refs");
        File.WriteAllText(".git/HEAD", "ref: refs/heads/main\n");
        Console.WriteLine("Initialized git directory");
        break;
    case "cat-file" when args[1] == "-p":
    {
        var contents = GitBlob.GetContent(Helpers.GetDecompressedBytes(args[2]));
        Console.Write(contents);
        break;
    }
    case "hash-object" when args[1] == "-w":
    {
        var blob = new GitBlob(File.ReadAllText(args[2]));
        var hash = Helpers.Compress(blob.UncompressedDataBytes);
        Console.Write(hash);
        break;
    }
    case "ls-tree" when args[1] == "--name-only":
    {
        var fullFileContentBytes = Helpers.GetDecompressedBytes(args[2]);
        var headerBytes = fullFileContentBytes.TakeWhile(b => b is not 0x00);
        var header = Encoding.UTF8.GetString(headerBytes.ToArray()).Split(' ');

        var treeEntries = new List<GitTreeEntry>();
        var size = int.Parse(header[1]);
        var contentsWithoutHeader = fullFileContentBytes[^size..];

        var lastNullByteIndex = -1;
        var lastSpaceByteIndex = -1;
        for (var i = 0; i < contentsWithoutHeader.Length; i++)
        {
            var currentByte = contentsWithoutHeader[i];

            switch (currentByte)
            {
                case 0x20:
                    lastSpaceByteIndex = i;
                    break;
                case 0x00:
                {
                    var modeStart = lastNullByteIndex == -1 ? 0 : lastNullByteIndex + 21;
                    var modeBytes = contentsWithoutHeader[modeStart..lastSpaceByteIndex];
                    var mode = Encoding.UTF8.GetString(modeBytes);
                    var type = mode.StartsWith("40") ? "tree" : "blob";

                    var nameBytes = contentsWithoutHeader[++lastSpaceByteIndex..i];
                    var name = Encoding.UTF8.GetString(nameBytes);

                    lastNullByteIndex = i;

                    var hashStart = i + 1;
                    var hashEnd = hashStart + 20;
                    var hashBytes = contentsWithoutHeader[hashStart..hashEnd];
                    var hash = Convert.ToHexStringLower(hashBytes);

                    treeEntries.Add(new GitTreeEntry(hash, mode, type, name));
                    break;
                }
            }
        }

        foreach (var treeEntry in treeEntries) Console.WriteLine(treeEntry.Name);

        break;
    }
    case "write-tree":
    {
        var workingDir = Path.Combine(Directory.GetCurrentDirectory());
        var tree = Helpers.GetTreeRecursive(workingDir);
        var hash = Helpers.Compress(tree.UncompressedDataBytes);
        Console.Write(hash);
        break;
    }
    case "commit-tree" when args[2] == "-p" && args[4] == "-m":
    {
        var commit = new GitCommit(args[1], args[3], args[5]);
        var hash = Helpers.Compress(commit.UncompressedDataBytes);
        Console.Write(hash);
        break;
    }
    case "clone":
    {
        var repoUrl = args[1];
        Directory.CreateDirectory(args[2]);
        Console.Error.WriteLine($"Cloning {repoUrl} into {args[2]}");
        break;
    }
    default:
        throw new ArgumentException($"Unknown command {command}");
}