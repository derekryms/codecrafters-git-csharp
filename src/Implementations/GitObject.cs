using System.Text;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class GitObject(ObjectType type, byte[] contentBytes)
{
    public string Type => type.ToString().ToLower();
    public byte[] ContentBytes => contentBytes;
    public string AsciiContent => Encoding.ASCII.GetString(ContentBytes);
}