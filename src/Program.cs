using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

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

        var filePathAndName = string.Empty;
        switch (option)
        {
            case "-p":
                filePathAndName = Path.Combine(".git", "objects", content[..2], content[2..]);
                break;
        }

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
    case "hash-object":
    {
        var option = args[1];
        var fileName = args[2];
        var contents = File.ReadAllText(fileName);
        var blobText = $"blob\x20{contents.Length}\0{contents}";
        var bytes = Encoding.UTF8.GetBytes(blobText);
        var sha1 = SHA1.HashData(bytes);
        var hash = Convert.ToHexStringLower(sha1);
        Console.Write(hash);

        switch (option)
        {
            case "-w":
                var firstTwoHash = hash[..2];
                var compressedFileName = hash[2..];
                Directory.CreateDirectory($".git/objects/{firstTwoHash}");

                using (var compressedFileStream = File.Create($".git/objects/{firstTwoHash}/{compressedFileName}"))
                {
                    using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
                    compressor.Write(bytes);
                }
                break;
        }
        break;
    }
    default:
        throw new ArgumentException($"Unknown command {command}");
}