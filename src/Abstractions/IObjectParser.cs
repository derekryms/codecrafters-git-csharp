using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IObjectParser
{
    (ObjectType type, string content) ParseGitObject(byte[] decompressedBytes);
}