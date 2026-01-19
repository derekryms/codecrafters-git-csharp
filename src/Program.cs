using System.IO.Compression;
using System.Text;
using codecrafters_git;

if (args.Length < 1)
{
    Console.WriteLine("Please provide a command.");
    return;
}

var command = args[0];

if (command == "init")
{
    Directory.CreateDirectory(".git");
    Directory.CreateDirectory(".git/objects");
    Directory.CreateDirectory(".git/refs");
    File.WriteAllText(".git/HEAD", "ref: refs/heads/main\n");
    Console.WriteLine("Initialized git directory");
}
else if (command == "cat-file" && args[1] == "-p")
{
    var contents = GitBlob.GetContent(Helpers.GetDecompressedBytes(args[2]));
    Console.Write(contents);
}
else if (command == "hash-object" && args[1] == "-w")
{
    var blob = new GitBlob(File.ReadAllText(args[2]));
    var hash = Helpers.GetSha1Hash(blob.UncompressedDataBytes);
    Console.WriteLine(hash);

    var fileDirectory = hash[..2];
    var compressedFileName = hash[2..];
    Directory.CreateDirectory($".git/objects/{fileDirectory}");

    using var compressedFileStream = File.Create($".git/objects/{fileDirectory}/{compressedFileName}");
    using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
    compressor.Write(blob.UncompressedDataBytes);
}
else if (command == "ls-tree"  && args[1] == "--name-only")
{
    var fullFileContentBytes = Helpers.GetDecompressedBytes(args[2]);
    var headerBytes = fullFileContentBytes.TakeWhile(b => b is not 0x00);
    var header = Encoding.UTF8.GetString(headerBytes.ToArray()).Split('\u0020');

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
    
    foreach (var treeEntry in treeEntries)
    {
        Console.WriteLine(treeEntry.Name);
    }
}
else if (command == "write-tree")
{
    var workingDir = Path.Combine(Directory.GetCurrentDirectory()/*, "testArea"*/);
    var tree = Helpers.GetTreeRecursive(workingDir);
    var hash = Helpers.GetSha1Hash(tree.UncompressedDataBytes);
    Console.Write(hash);
    
    var fileDirectory = hash[..2];
    var compressedFileName = hash[2..];
    Directory.CreateDirectory($".git/objects/{fileDirectory}");

    using var compressedFileStream = File.Create($".git/objects/{fileDirectory}/{compressedFileName}");
    using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
    compressor.Write(tree.UncompressedDataBytes);
}
else if (command == "commit-tree" && args[2] == "-p" && args[4] == "-m")
{
    var commit = new GitCommit(args[1], args[3], args[5]);
    var hash = Helpers.GetSha1Hash(commit.UncompressedDataBytes);
    Console.Write(hash);
    
    var fileDirectory = hash[..2];
    var compressedFileName = hash[2..];
    Directory.CreateDirectory($".git/objects/{fileDirectory}");

    using var compressedFileStream = File.Create($".git/objects/{fileDirectory}/{compressedFileName}");
    using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
    compressor.Write(commit.UncompressedDataBytes);
}
else if (command == "clone")
{
    var repoUrl = args[1];
    Directory.CreateDirectory(args[2]);
    Console.Error.WriteLine($"Cloning {repoUrl} into {args[2]}");
}
else
{
    throw new ArgumentException($"Unknown command {command}");
}