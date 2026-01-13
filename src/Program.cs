using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

if (args.Length < 1)
{
    Console.WriteLine("Please provide a command.");
    return;
}

// You can use print statements as follows for debugging, they'll be visible when running tests.
Console.Error.WriteLine("Logs from your program will appear here!");

var command = args[0];
var option = args.ElementAtOrDefault(1);
var specifier = args.ElementAtOrDefault(2);

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
        switch (option)
        {
            case "-p":
                if (specifier is null)
                    throw new ArgumentException($"No hash after {option} specified");
                
                var fullFileContentBytes = GetGitObjectBytesFromHashHex(specifier);
                var userFileContents = Encoding.UTF8.GetString(fullFileContentBytes).Split('\0')[1];
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
    case "ls-tree":
    {
        if (specifier is null)
            throw new ArgumentException($"No hash after {option} specified");
                
        var fullFileContentBytes = GetGitObjectBytesFromHashHex(specifier);
        var headerBytes = fullFileContentBytes.TakeWhile(b => b is not 0x00);
        var header = Encoding.UTF8.GetString(headerBytes.ToArray()).Split('\u0020');
        var type = header[0];
        if (type is not "tree")
            throw new ArgumentException($"Invalid hash type {type}");

        List<GitTreeObject> treeObjects = [];
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
                    var mode = int.Parse(Encoding.UTF8.GetString(modeBytes));
                        
                    var nameBytes = contentsWithoutHeader[++lastSpaceByteIndex..i];
                    var name = Encoding.UTF8.GetString(nameBytes);
                        
                    lastNullByteIndex = i;
                        
                    var hashStart = i + 1;
                    var hashEnd = hashStart + 20;
                    var hash = contentsWithoutHeader[hashStart..hashEnd];
                    var sha1Hash = SHA1.HashData(hash);
                    treeObjects.Add(new GitTreeObject(sha1Hash, name, mode));
                    break;
                }
            }
        }
                
        var orderedTreeObjects = treeObjects.OrderBy(o => o.Name);
        switch (option)
        {
            case "--name-only":
                var sb = new StringBuilder();
                foreach (var treeObject in orderedTreeObjects)
                {
                    sb.AppendLine(treeObject.Name);
                }

                Console.WriteLine(sb.ToString().TrimEnd());
                break;
        }
        break;
    }
    default:
        throw new ArgumentException($"Unknown command {command}");
}

return;

byte[] GetGitObjectBytesFromHashHex(string hashHex)
{
    var decompressedFileStream = new MemoryStream();
    using (var compressedFileStream = File.Open($".git/objects/{hashHex[..2]}/{hashHex[2..]}", FileMode.Open, FileAccess.Read))
    {
        using var decompressor = new ZLibStream(compressedFileStream, CompressionMode.Decompress);
        decompressor.CopyTo(decompressedFileStream);
    }

    return decompressedFileStream.ToArray();
}

class GitTreeObject(byte[] sha1Hash, string name, int mode)
{
    public byte[] Sha1Hash { get; } = sha1Hash;
    public string Name { get; } = name;
    public int Mode { get; } = mode;
}