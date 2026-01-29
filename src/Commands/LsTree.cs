using System.Text;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Commands;

public class LsTree(string rootDirectory) : ICommand
{
    public Task Run(string[] args)
    {
        var option = args[0];
        var blobHash = args[1];

        if (option == "--name-only")
        {
            var fullFileContentBytes = Helpers.GetDecompressedBytes(rootDirectory, blobHash);
            var headerBytes = fullFileContentBytes.TakeWhile(b => b is not 0x00);
            var header = Encoding.UTF8.GetString(headerBytes.ToArray()).Split(' ');

            var treeEntries = new List<GitTreeEntry>();
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
                        var mode = Encoding.UTF8.GetString(modeBytes);
                        var type = mode.StartsWith("40") ? "tree" : "blob";

                        var nameBytes = contentsWithoutHeader[++lastSpaceByteIndex..i];
                        var name = Encoding.UTF8.GetString(nameBytes);

                        lastNullByteIndex = i;

                        var hashStart = i + 1;
                        var hashEnd = hashStart + 20;
                        var hashBytes = contentsWithoutHeader[hashStart..hashEnd];
                        var treeHash = Convert.ToHexStringLower(hashBytes);

                        treeEntries.Add(new GitTreeEntry(treeHash, mode, name));
                        break;
                    }
                }
            }

            foreach (var treeEntry in treeEntries) Console.WriteLine(treeEntry.Name);
        }
        
        return Task.CompletedTask;
    }
}