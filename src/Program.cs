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
var option = args.ElementAtOrDefault(1);

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
        var hash = args.ElementAtOrDefault(2);

        switch (option)
        {
            case "-p":
                if (hash is null)
                    throw new ArgumentException($"No hash after {option} specified");
                
                var decompressedFileStream = new MemoryStream();
                using (var compressedFileStream = File.Open($".git/objects/{hash[..2]}/{hash[2..]}", FileMode.Open, FileAccess.Read))
                {
                    using var decompressor = new ZLibStream(compressedFileStream, CompressionMode.Decompress);
                    decompressor.CopyTo(decompressedFileStream);
                }

                var fullFileContents = Encoding.UTF8.GetString(decompressedFileStream.ToArray());
                var userFileContents = fullFileContents.Split('\0')[1];
                Console.Write(userFileContents);
                break;
        }
        break;
    }
    case "hash-object":
    {
        var fileName = args.ElementAtOrDefault(2);
        if (fileName is null)
            throw new ArgumentException("No file name provided");
        
        var userFileContents = File.ReadAllText(fileName);
        var blobText = $"blob\x20{userFileContents.Length}\0{userFileContents}";
        var fullFileBytes = Encoding.UTF8.GetBytes(blobText);
        var sha1Hash = SHA1.HashData(fullFileBytes);
        var sha1HashHex = Convert.ToHexStringLower(sha1Hash);
        Console.Write(sha1HashHex);

        switch (option)
        {
            case "-w":
                var fileDirectory = sha1HashHex[..2];
                var compressedFileName = sha1HashHex[2..];
                Directory.CreateDirectory($".git/objects/{fileDirectory}");

                using (var compressedFileStream = File.Create($".git/objects/{fileDirectory}/{compressedFileName}"))
                {
                    using var compressor = new ZLibStream(compressedFileStream, CompressionMode.Compress);
                    compressor.Write(fullFileBytes);
                }
                break;
        }
        break;
    }
    default:
        throw new ArgumentException($"Unknown command {command}");
}