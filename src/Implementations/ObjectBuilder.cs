using System.Security.Cryptography;
using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class ObjectBuilder(IFileSystem fileSystem) : IObjectBuilder
{
    public byte[] BuildGitObject(ObjectType type, byte[] content)
    {
        var header = $"{type.ToString().ToLower()} {content.Length}\0";
        var headerBytes = Encoding.ASCII.GetBytes(header);
        return [..headerBytes, ..content];
    }

    public byte[] BuildTreeObjectContentBytes(string directory)
    {
        var entries = new List<(string Name, byte[] EntryBytes)>();
        var files = fileSystem.GetFiles(directory);
        var directories = fileSystem.GetDirectories(directory).Where(d => !d.EndsWith(".git"));

        foreach (var file in files)
        {
            var fileBytes = fileSystem.ReadAllBytes(file);
            var fileName = fileSystem.GetFileName(file);
            var gitObjectBytes = BuildGitObject(ObjectType.Blob, fileBytes);
            var hashBytes = SHA1.HashData(gitObjectBytes);
            var prefix = $"100644 {fileName}\0";
            var prefixBytes = Encoding.ASCII.GetBytes(prefix);
            entries.Add((fileName, [..prefixBytes, ..hashBytes]));
        }

        foreach (var dir in directories)
        {
            var directoryName = fileSystem.GetFileName(dir);
            var prefix = $"40000 {directoryName}\0";
            var treeBytes = BuildTreeObjectContentBytes(dir);
            var gitObjectBytes = BuildGitObject(ObjectType.Tree, treeBytes);
            var hashBytes = SHA1.HashData(gitObjectBytes);
            var prefixBytes = Encoding.ASCII.GetBytes(prefix);
            entries.Add((directoryName, [..prefixBytes, ..hashBytes]));
        }

        return entries.OrderBy(e => e.Name).SelectMany(e => e.EntryBytes).ToArray();
    }

    public byte[] BuildCommitObjectContentBytes(string treeHash, string parentHash, string message)
    {
        var treeLine = $"tree {treeHash}\n";
        var parentLine = $"parent {parentHash}\n";
        var authorCommitterLine = "author Derek Ryms <dryms@arielcorp.com> 1770343328 -0500\n";
        var messageLine = $"\n{message}\n";

        var commitContent = treeLine + parentLine + authorCommitterLine + messageLine;
        return Encoding.ASCII.GetBytes(commitContent);
    }
}