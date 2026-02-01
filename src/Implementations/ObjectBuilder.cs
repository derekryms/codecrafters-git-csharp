using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectBuilder : IObjectBuilder
{
    public byte[] BuildGitObject(ObjectType type, byte[] content)
    {
        var header = $"{type.ToString().ToLower()} {content.Length}\0";
        var headerBytes = Encoding.UTF8.GetBytes(header);
        return [..headerBytes, ..content];
    }
}