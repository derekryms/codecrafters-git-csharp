using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IObjectBuilder
{
    byte[] BuildGitObject(ObjectType type, byte[] content);
    byte[] BuildTreeObjectContentBytes(string directory);
    byte[] BuildCommitObjectContentBytes(string treeHash, string parentHash, string message);
}