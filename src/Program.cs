using System.IO.Compression;

if (args.Length < 1)
{
    Console.WriteLine("Please provide a command.");
    return;
}

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");

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
    case "cat-file":
    {
        var option = args[1];
        var content = args[2];

        var filePathAndName = option switch
        {
            "-p" => Path.Combine(".git", "objects", content[..2], content[2..]),
            _ => string.Empty
        };

        const string decompressedFileName = "test";
        using (var compressedFileStream = File.Open(filePathAndName, FileMode.Open, FileAccess.Read))
        {
            using var outputFileStream = File.Create(decompressedFileName);
            using var decompressor = new ZLibStream(compressedFileStream, CompressionMode.Decompress);
            decompressor.CopyTo(outputFileStream);
        }

        var contents = File.ReadAllText(decompressedFileName).Split('\0')[1];
        Console.Write(contents);
        break;
    }
    default:
        throw new ArgumentException($"Unknown command {command}");
}