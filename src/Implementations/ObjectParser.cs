using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectParser : IObjectParser
{
    public GitObject ParseGitObject(byte[] decompressedBytes)
    {
        for (var i = 0; i < decompressedBytes.Length; i++)
        {
            var currentByte = decompressedBytes[i];
            if (currentByte is not Constants.NullByte)
            {
                continue;
            }

            var headerBytes = decompressedBytes[..i];
            var header = ParseGitObjectHeader(headerBytes);

            var contentBytes = decompressedBytes[(i + 1)..];

            return new GitObject(header, contentBytes);
        }

        throw new ArgumentException("Invalid git object: no null byte found.");
    }

    /*
    blob <size>\0<content>
    */
    public Blob ParseBlobObject(byte[] blobContentBytes)
    {
        var asciiContent = Encoding.ASCII.GetString(blobContentBytes);
        return new Blob(asciiContent);
    }

    /*
    tree <size>\0
    <mode> <name>\0<20_byte_sha>
    <mode> <name>\0<20_byte_sha>
    */
    public Tree ParseTreeObject(byte[] treeContentBytes)
    {
        var treeEntries = new List<TreeEntry>();

        var spaceByteIndex = 0;
        var entryEndIndex = 0;
        var i = 0;
        while (i < treeContentBytes.Length)
        {
            var currentByte = treeContentBytes[i];
            if (currentByte is Constants.SpaceByte)
            {
                spaceByteIndex = i;
                i++;
                continue;
            }

            if (currentByte is not Constants.NullByte)
            {
                i++;
                continue;
            }

            var nullByteIndex = i;
            var shaStartIndex = nullByteIndex + 1;

            var modeBytes = treeContentBytes[entryEndIndex..spaceByteIndex];
            var mode = GetModeFromBytes(modeBytes);

            var nameBytes = treeContentBytes[(spaceByteIndex + 1)..nullByteIndex];
            var name = Encoding.ASCII.GetString(nameBytes);

            entryEndIndex = shaStartIndex + Constants.ShaByteLength;
            var shaBytes = treeContentBytes[shaStartIndex..entryEndIndex];
            var sha = Convert.ToHexStringLower(shaBytes);

            treeEntries.Add(new TreeEntry(mode, sha, name));

            i = entryEndIndex;
        }

        return new Tree(treeEntries);
    }

    private static ObjectHeader ParseGitObjectHeader(byte[] headerBytes)
    {
        for (var i = 0; i < headerBytes.Length; i++)
        {
            var headerByte = headerBytes[i];
            if (headerByte is not Constants.SpaceByte)
            {
                continue;
            }

            var typeBytes = headerBytes[..i];
            var type = GetTypeFromBytes(typeBytes);

            var lengthBytes = headerBytes[(i + 1)..];
            var length = GetLengthFromBytes(lengthBytes);

            return new ObjectHeader(type, length);
        }

        throw new ArgumentException("Invalid git object header: no space byte found.");
    }

    private static ObjectType GetTypeFromBytes(byte[] typeBytes)
    {
        var typeString = Encoding.ASCII.GetString(typeBytes);
        return typeString switch
        {
            "blob" => ObjectType.Blob,
            "tree" => ObjectType.Tree,
            "commit" => ObjectType.Commit,
            "tag" => ObjectType.Tag,
            _ => throw new ArgumentException($"Unsupported git object type: {typeString}")
        };
    }

    private static int GetLengthFromBytes(byte[] lengthBytes)
    {
        var lengthString = Encoding.ASCII.GetString(lengthBytes);
        var valid = int.TryParse(lengthString, out var length);
        return !valid ? throw new ArgumentException($"Invalid length bytes: {lengthString}") : length;
    }

    private static TreeEntryMode GetModeFromBytes(byte[] modeBytes)
    {
        var modeString = Encoding.ASCII.GetString(modeBytes);
        return modeString switch
        {
            "100644" => TreeEntryMode.RegularFile,
            "100755" => TreeEntryMode.ExecutableFile,
            "120000" => TreeEntryMode.SymbolicLink,
            "40000" => TreeEntryMode.Directory,
            _ => throw new ArgumentException($"Unsupported tree entry type: {modeString}")
        };
    }
}