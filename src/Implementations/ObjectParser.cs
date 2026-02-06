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
            var type = ParseTypeFromHeader(headerBytes);

            var contentBytes = decompressedBytes[(i + 1)..];

            return new GitObject(type, contentBytes);
        }

        throw new ArgumentException("Invalid git object: no null byte found.");
    }

    /*
    tree <size>\0
    <mode> <name>\0<20_byte_sha>
    <mode> <name>\0<20_byte_sha>
    */
    public Tree ParseTreeObject(byte[] treeContentBytes)
    {
        var treeEntries = new List<TreeEntry>();

        var entryEndIndex = 0;
        var i = 0;
        while (i < treeContentBytes.Length)
        {
            var spaceByteIndex = FindSpaceByte(treeContentBytes, i);
            var nullByteIndex = FindNullByte(treeContentBytes, spaceByteIndex);

            var modeBytes = treeContentBytes[entryEndIndex..spaceByteIndex];
            var mode = GetModeFromBytes(modeBytes);

            var nameBytes = treeContentBytes[(spaceByteIndex + 1)..nullByteIndex];
            var name = Encoding.ASCII.GetString(nameBytes);

            var shaStartIndex = nullByteIndex + 1;
            entryEndIndex = shaStartIndex + Constants.ShaByteLength;
            var shaBytes = treeContentBytes[shaStartIndex..entryEndIndex];
            var sha = Convert.ToHexStringLower(shaBytes);

            treeEntries.Add(new TreeEntry(mode, sha, name));

            i = entryEndIndex;
        }

        return new Tree(treeEntries);
    }

    private static ObjectType ParseTypeFromHeader(byte[] headerBytes)
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

            // var lengthBytes = headerBytes[(i + 1)..];
            // var length = GetLengthFromBytes(lengthBytes);

            return type;
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

    // private static int GetLengthFromBytes(byte[] lengthBytes)
    // {
    //     var lengthString = Encoding.ASCII.GetString(lengthBytes);
    //     var valid = int.TryParse(lengthString, out var length);
    //     return !valid ? throw new ArgumentException($"Invalid length bytes: {lengthString}") : length;
    // }

    private static int FindSpaceByte(byte[] treeContentBytes, int i)
    {
        // Find space byte (separates mode from name)
        var spaceByteIndex = -1;
        for (var j = i; j < treeContentBytes.Length; j++)
        {
            if (treeContentBytes[j] != Constants.SpaceByte)
            {
                continue;
            }

            spaceByteIndex = j;
            break;
        }

        if (spaceByteIndex == -1)
        {
            throw new ArgumentException("Invalid tree entry: no space byte found.");
        }

        return spaceByteIndex;
    }

    private static int FindNullByte(byte[] treeContentBytes, int spaceByteIndex)
    {
        // Find null byte (separates name from SHA)
        var nullByteIndex = -1;
        for (var j = spaceByteIndex + 1; j < treeContentBytes.Length; j++)
        {
            if (treeContentBytes[j] != Constants.NullByte)
            {
                continue;
            }

            nullByteIndex = j;
            break;
        }

        if (nullByteIndex == -1)
        {
            throw new ArgumentException("Invalid tree entry: no null byte found.");
        }

        return nullByteIndex;
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