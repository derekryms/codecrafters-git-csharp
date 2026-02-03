using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;

namespace codecrafters_git.Abstractions;

public interface IObjectParser
{
    GitObject ParseGitObject(byte[] decompressedBytes);
    Blob ParseBlobObject(byte[] blobContentBytes);
    Tree ParseTreeObject(byte[] treeContentBytes);
}