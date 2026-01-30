using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using codecrafters_git.GitObjects;
// using ICSharpCode.SharpZipLib.Zip.Compression;

namespace codecrafters_git.Commands;

public class Clone : ICommand
{
    private readonly Dictionary<string, GitType> _hashToType = new();
    
    public async Task Run(string[] args)
    {
        var repoUrl = args[0];
        var destination = args[1];
        var rootDirectory = $"{destination}/";

        Directory.CreateDirectory(destination);
        await new Init(rootDirectory).Run([]);

        using var httpClient = new HttpClient();
        var initialServerRequestUrl = $"{repoUrl}/info/refs?service=git-upload-pack";
        var response = await httpClient.GetAsync(initialServerRequestUrl);
        var responseString = await response.Content.ReadAsStringAsync();
        
        var content = new List<string>();
        var pos = 0;

        while (pos < responseString.Length - 1)
        {
            var lineLengthStart = pos;
            var lineLengthEnd = pos + 4;
            var lineLenght = Convert.ToInt32(responseString.Substring(lineLengthStart, 4), 16);
            if (lineLenght == 0)
            {
                pos += 4;
                continue;
            }
            content.Add(responseString.Substring(lineLengthEnd, lineLenght - 4));
            pos += lineLenght;
        }

        var headHex = content[1].Split(" HEAD")[0];
        
        var packServerRequestUrl = $"{repoUrl}/git-upload-pack";
        var pktLine = Helpers.GetPktLine($"want {headHex}");
        var donePktLine = Helpers.GetPktLine("done");
        var body = pktLine + "0000" + donePktLine;
        var byteArrayContent = new ByteArrayContent(Encoding.UTF8.GetBytes(body));
        response = await httpClient.PostAsync(packServerRequestUrl, byteArrayContent);
        var responseBytes = await response.Content.ReadAsByteArrayAsync();
        
        var packDataStart = GetPackDataStart(responseBytes);
        var headerEnd = packDataStart + 12;
        var endPos = responseBytes.Length - 20;

        var types = new List<GitType>();
        pos = headerEnd;
        while (pos < endPos)
        {
            var (gitType, size) = ReadTypeAndSize(responseBytes, ref pos);
            var baseObjectType = gitType;
            types.Add(gitType);

            byte[] decompressedData = [];
            if (gitType != GitType.RefDelta && gitType != GitType.OfsDelta)
            {
                decompressedData = DecompressObject(responseBytes, ref pos, responseBytes.Length - pos - 20);
            }
            else
            {
                var deltaHash = Convert.ToHexStringLower(responseBytes[pos..(pos + 20)]);
                pos += 20;
                decompressedData = DecompressObject(responseBytes, ref pos, responseBytes.Length - pos - 20);
                var deltaPos = 0;
                var sourceSize = ReadDeltaSize(decompressedData, ref deltaPos);
                var targetSize = ReadDeltaSize(decompressedData, ref deltaPos);
                var instructions = DeltaInstructions(decompressedData, ref deltaPos);
                
                
                baseObjectType = _hashToType[deltaHash];
                var targetObject = new byte[targetSize];
                var targetPos = 0;
                foreach (var instruction in instructions)
                {
                    switch (instruction)
                    {
                        case CopyInstruction copy:
                            var baseObject = Helpers.GetDecompressedBytes(rootDirectory, deltaHash);
                            var start = Array.IndexOf(baseObject, (byte)0) + 1;
                            var data = baseObject[start..(start + (int)sourceSize)];
                            Array.Copy(data, (int)copy.Offset, targetObject, targetPos, (int)copy.Size);
                            targetPos += (int)copy.Size;
                            break;
                        case InsertInstruction insert:
                            insert.Data.CopyTo(targetObject, targetPos);
                            targetPos += insert.Data.Length;
                            break;
                    }
                }
                
                switch (baseObjectType)
                {
                    case GitType.Commit:
                        var commit = new GitCommit(targetObject);
                        decompressedData = commit.UncompressedDataBytes;
                        break;
                    case GitType.Tree:
                        var tree = new GitTree(targetObject);
                        decompressedData = tree.UncompressedDataBytes;
                        break;
                    case GitType.Blob:
                        var blob = new GitBlob(targetObject);
                        decompressedData = blob.UncompressedDataBytes;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            var hash = "";
            switch (gitType)
            {
                case GitType.Commit:
                    var commit = new GitCommit(decompressedData);
                    hash = Helpers.Compress(rootDirectory, commit.UncompressedDataBytes);
                    break;
                case GitType.Tree:
                    var tree = new GitTree(decompressedData);
                    hash = Helpers.Compress(rootDirectory, tree.UncompressedDataBytes);
                    break;
                case GitType.Blob:
                    var blob = new GitBlob(decompressedData);
                    hash = Helpers.Compress(rootDirectory, blob.UncompressedDataBytes);
                    break;
                case GitType.RefDelta:
                    hash = Helpers.Compress(rootDirectory, decompressedData);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            _hashToType[hash] = baseObjectType;
        }

        var firstCommitHash = _hashToType.First(t => t.Value is GitType.Commit).Key;
        var commitContent = Encoding.UTF8.GetString(Helpers.GetDecompressedBytes(rootDirectory, firstCommitHash));
        var treeHash = commitContent.Split('\0')[1].Split('\n')[0].Remove(0, 5);

        CheckoutTree(rootDirectory, treeHash, destination);
    }

    private static void CheckoutTree(string rootDirectory, string treeHash, string destination)
    {
        var treeBytes = Helpers.GetDecompressedBytes(rootDirectory, treeHash);
        var treeEntries = GitTree.GetTreeEntries(treeBytes);

        foreach (var treeEntry in treeEntries)
        {
            var targetPath = Path.Combine(destination, treeEntry.Name);

            if (treeEntry.Mode.Contains("40000"))
            {
                Directory.CreateDirectory(targetPath);
                CheckoutTree(rootDirectory, treeEntry.Sha1Hash, targetPath);
            }
            else if (treeEntry.Mode.Contains('1'))
            {
                var blobBytes = Helpers.GetDecompressedBytes(rootDirectory, treeEntry.Sha1Hash);
                var content = Encoding.UTF8.GetString(blobBytes).Split('\0')[1];
                Directory.CreateDirectory(destination);
                File.WriteAllText(targetPath, content);
            }
        }
    }

    // Not gonna lie, copied this from https://github.com/ba2ooz/codecrafters-git-csharp
    private byte[] DecompressObject(byte[] pack, ref int position, int maxLength)
    {
        using var output = new MemoryStream();
        // var buffer = new byte[4096];
        //
        // var inflater = new Inflater();
        // inflater.SetInput(pack, position, maxLength);
        //
        // while (inflater is { IsFinished: false, IsNeedingInput: false })
        // {
        //     var count = inflater.Inflate(buffer);
        //     if (count == 0 && inflater.IsNeedingInput)
        //         break;
        //     
        //     output.Write(buffer, 0, count);
        // }
        //
        // position += (int) inflater.TotalIn;
        return output.ToArray();
    }
    
    private static (GitType gitType, ulong size) ReadTypeAndSize(byte[] bytes, ref int pos)
    {
        var firstByte = true;
        var lastByte = false;
        var lengthBytes = new List<byte>();
        var gitType = (GitType)0;
        while (pos < bytes.Length - 1)
        {
            var byteValue = bytes[pos];
            pos++;
            if (firstByte)
            {
                gitType = GetGitType(byteValue);
                var value = GetFirstValue(byteValue);
                lengthBytes.Add(value);
                firstByte = false;
                if (byteValue > 128)
                {
                    continue;
                }

                lastByte = true;
            }
            else
            {
                if (byteValue > 128)
                {
                    var value = GetValue(byteValue);
                    lengthBytes.Add(value);
                    continue;
                }
                else
                {
                    var value = GetValue(byteValue);
                    lengthBytes.Add(value);
                    lastByte = true;
                }
            }
        
            if (lastByte)
            {
                var size = (ulong)lengthBytes[0];
                for (var i = 1; i < lengthBytes.Count; i++)
                {
                    var shift = i == 1 ? 4 : 7 * i;
                    var shifted = lengthBytes[i] << shift;
                    size |= (ulong)shifted;
                }

                return (gitType, size);
            }
        }
        
        throw new Exception("Could not read type and size");
    }

    private static ulong ReadDeltaSize(byte[] delta, ref int pos)
    {
        var size = (ulong)0;
        var shift = 0;
        while (pos < delta.Length)
        {
            var currentByte = delta[pos];
            var moreBytes = (currentByte & 0x80) != 0; // 0b1000_0000 mask means more bytes
            var length = (ulong)(currentByte & 0x7F) << shift; // 0b0111_1111 mask to get the lower 7 bits
            size |= length;
            pos++;
            shift += 7;
            
            if (!moreBytes)
                break;
        }
        
        return size;
    }
    
    private abstract record Instruction;
    private record CopyInstruction(ulong Offset, ulong Size) : Instruction;
    private record InsertInstruction(byte[] Data) : Instruction;
    
    private static List<Instruction> DeltaInstructions(byte[] delta, ref int pos)
    {
        var instructions = new List<Instruction>();
        while (pos < delta.Length)
        {
            var currentByte = delta[pos];
            if ((currentByte & 0x80) != 0) // Copy instruction
            {
                var offset1 = (currentByte & 0x01) != 0;
                var offset2 = (currentByte & 0x02) != 0;
                var offset3 = (currentByte & 0x04) != 0;
                var offset4 = (currentByte & 0x08) != 0;
                bool[] offsets = [ offset1, offset2, offset3, offset4 ];
                var size1 = (currentByte & 0x10) != 0;
                var size2 = (currentByte & 0x20) != 0;
                var size3 = (currentByte & 0x40) != 0;
                bool[] sizes = [ size1, size2, size3 ];
                pos++;

                var totalOffset = GetEncodedValue(offsets, delta, ref pos);
                var totalSize = GetEncodedValue(sizes, delta, ref pos);
                instructions.Add(new CopyInstruction(totalOffset, totalSize));
            }
            else // Insert instruction
            {
                var size = (ulong)currentByte;
                pos++;
                instructions.Add(new InsertInstruction(delta[pos..(pos + (int)size)]));
                pos += (int)size;
            }
        }

        return instructions;
    }

    private static ulong GetEncodedValue(bool[] values, byte[] delta, ref int pos)
    {
        var totalValue = (ulong)0;
        var shift = 0;
        foreach (var value in values)
        {
            if (value)
            {
                totalValue |= (ulong)delta[pos] << shift;
                pos++;
            }
            shift += 8;
        }
        return totalValue;
    }
    
    private static int GetPackDataStart(byte[] bytes)
    {
        for (var i = 0; i < bytes.Length - 1; i++)
        {
            if (bytes[i] == (byte)'P' && bytes[i + 1] == (byte)'A' && bytes[i + 2] == (byte)'C' && bytes[i + 3] == (byte)'K')
            {
                return i;
            }
        }
        
        throw new Exception("No pack data found");
    }

    private static GitType GetGitType(byte hByte)
    {
        var shifted = (byte)(hByte >> 4);
        var type = shifted & 0b0000_0111;
        return (GitType)type;
    }
    
    private static byte GetFirstValue(byte hByte)
    {
        var value = (byte)(hByte & 0b0000_1111);
        return value;
    }
    
    private static byte GetValue(byte hByte)
    {
        var value = (byte)(hByte & 0b0111_1111);
        return value;
    }

    private enum GitType
    {
        Commit = 1,
        Tree = 2,
        Blob = 3,
        Tag = 4,
        OfsDelta = 6,
        RefDelta = 7
    }
}