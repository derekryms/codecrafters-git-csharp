using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectParser : IObjectParser
{
    public (ObjectType type, string content) ParseGitObject(byte[] decompressedBytes)
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
            var content = Encoding.ASCII.GetString(contentBytes);
            return (header.Type, content);
        }

        throw new ArgumentException("Invalid git object: no null byte found.");
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
}