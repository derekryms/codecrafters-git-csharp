using System.Text;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace codecrafters_git.Implementations;

public class PackParser : IPackParser
{
    public Pack ParsePackBytes(byte[] negotiationBytes)
    {
        var objectBytes = ParsePackMetaInfo(negotiationBytes).PackObjectBytes;
        var pack = ParsePackObjects(objectBytes);
        return pack;
    }

    private static PackMetaInfo ParsePackMetaInfo(byte[] bytes)
    {
        for (var i = 0; i < bytes.Length - Constants.PackHeader.Length; i++)
        {
            if (bytes[i] == Constants.PackHeader[0] &&
                bytes[i + 1] == Constants.PackHeader[1] &&
                bytes[i + 2] == Constants.PackHeader[2] &&
                bytes[i + 3] == Constants.PackHeader[3])
            {
                // The version and pack objects count are 4 bytes each, network byte order (big endian)
                var verBytes = bytes[(i + 4)..(i + 8)];
                var version = (verBytes[0] << 24) | (verBytes[1] << 16) | (verBytes[2] << 8) | verBytes[3];
                var cntBytes = bytes[(i + 8)..(i + 12)];
                var packObjectsCount = (cntBytes[0] << 24) | (cntBytes[1] << 16) | (cntBytes[2] << 8) | cntBytes[3];
                var packSha = Convert.ToHexStringLower(bytes[^Constants.ShaByteLength..]);
                return new PackMetaInfo(version, packObjectsCount, packSha, bytes[(i + 12)..^Constants.ShaByteLength]);
            }
        }

        throw new ArgumentException("Not a valid pack negotiation.");
    }

    private static Pack ParsePackObjects(byte[] bytes)
    {
        var undeltifiedPackObjects = new List<UndeltifiedPackObject>();
        var refDeltaPackObjects = new List<RefDeltaPackObject>();
        var pos = 0;
        while (pos < bytes.Length)
        {
            // First byte
            var curByte = bytes[pos++];
            var type = (PackObjectType)((curByte & Constants.FirstByteTypeMask) >> Constants.FirstByteSizeShift);
            var size = curByte & Constants.FirstByteSizeMask;
            var shift = Constants.FirstByteSizeShift;

            // Size continues
            while ((curByte & Constants.MsbMask) == Constants.MsbMask)
            {
                curByte = bytes[pos++];
                size |= (curByte & Constants.NByteSizeMask) << shift;
                shift += Constants.NByteSizeShift;
            }

            switch (type)
            {
                case PackObjectType.Commit:
                case PackObjectType.Tree:
                case PackObjectType.Blob:
                case PackObjectType.Tag:
                    var uncompressedData = DecompressObjectData(bytes, size, ref pos);
                    undeltifiedPackObjects.Add(new UndeltifiedPackObject(type, uncompressedData,
                        Encoding.ASCII.GetString(uncompressedData)));
                    break;
                case PackObjectType.RefDelta:
                    refDeltaPackObjects.Add(ParseRefDeltaObject(bytes, size, ref pos));
                    break;
                // Not implementing ofs delta for now
                case PackObjectType.OfsDelta:
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        return new Pack(undeltifiedPackObjects, refDeltaPackObjects);
    }

    private static byte[] DecompressObjectData(byte[] bytes, int size, ref int pos)
    {
        // Undeltified object data is the compressed zlib data
        // The size in the object header is that of the uncompressed data
        // Need to know the number of compressed bytes read to update the position in the pack data correctly
        // ZLibStream does not provide this information, but SharpZipLib's Inflater does through the TotalIn property
        using var compressedStream = new MemoryStream(bytes, pos, bytes.Length - pos, false);
        var inflater = new Inflater();
        using var inflaterStream = new InflaterInputStream(compressedStream, inflater);
        using var decompressedStream = new MemoryStream();

        inflaterStream.CopyTo(decompressedStream);

        if (decompressedStream.Length != size)
        {
            throw new InvalidDataException(
                $"Decompressed data size {decompressedStream.Length} does not match expected size {size}.");
        }

        // This is the actual number of compressed bytes read
        pos += (int)inflater.TotalIn;

        return decompressedStream.ToArray();
    }

    private static RefDeltaPackObject ParseRefDeltaObject(byte[] bytes, int size, ref int pos)
    {
        var sha = Convert.ToHexStringLower(bytes[pos..(pos + Constants.ShaByteLength)]);
        pos += Constants.ShaByteLength;

        var objectData = DecompressObjectData(bytes, size, ref pos);

        var deltaPos = 0;
        var sourceSize = ParseRefDeltaSize(objectData, ref deltaPos);
        var targetSize = ParseRefDeltaSize(objectData, ref deltaPos);
        var instructions = ParseRefDeltaInstructions(objectData, ref deltaPos);

        return new RefDeltaPackObject(sha, sourceSize, targetSize, instructions);
    }

    private static int ParseRefDeltaSize(byte[] bytes, ref int pos)
    {
        while (pos < bytes.Length)
        {
            // First byte
            var curByte = bytes[pos++];
            var size = curByte & Constants.NByteSizeMask;
            var shift = Constants.NByteSizeShift;

            // Size continues
            while ((curByte & Constants.MsbMask) == Constants.MsbMask)
            {
                curByte = bytes[pos++];
                size |= (curByte & Constants.NByteSizeMask) << shift;
                shift += Constants.NByteSizeShift;
            }

            return size;
        }

        throw new ArgumentException("Invalid ref delta size encoding.");
    }

    private static List<RefDeltaInstruction> ParseRefDeltaInstructions(byte[] bytes, ref int pos)
    {
        var instructions = new List<RefDeltaInstruction>();
        while (pos < bytes.Length)
        {
            var curByte = bytes[pos];
            if ((curByte & Constants.MsbMask) == Constants.MsbMask)
            {
                var size3 = (curByte & Constants.ByteSize3Mask) == Constants.ByteSize3Mask;
                var size2 = (curByte & Constants.ByteSize2Mask) == Constants.ByteSize2Mask;
                var size1 = (curByte & Constants.ByteSize1Mask) == Constants.ByteSize1Mask;
                bool[] sizeBits = [size1, size2, size3];

                var offset4 = (curByte & Constants.ByteOffset4Mask) == Constants.ByteOffset4Mask;
                var offset3 = (curByte & Constants.ByteOffset3Mask) == Constants.ByteOffset3Mask;
                var offset2 = (curByte & Constants.ByteOffset2Mask) == Constants.ByteOffset2Mask;
                var offset1 = (curByte & Constants.ByteOffset1Mask) == Constants.ByteOffset1Mask;
                bool[] offsetBits = [offset1, offset2, offset3, offset4];

                var offset = ParseInstructionValue(bytes, offsetBits, ref pos);
                var size = ParseInstructionValue(bytes, sizeBits, ref pos);
                pos++;

                instructions.Add(new CopyInstruction(offset, size));
            }
            else
            {
                var size = curByte & Constants.RefByteInsertSizeMask;
                pos++;

                var bytesToCopy = bytes[pos..(pos + size)];
                instructions.Add(new InsertInstruction(bytesToCopy, Encoding.ASCII.GetString(bytesToCopy)));
                pos += size;
            }
        }

        return instructions;
    }

    private static int ParseInstructionValue(byte[] bytes, bool[] valueBits, ref int pos)
    {
        var value = 0;
        var shift = 0;
        foreach (var valueBit in valueBits)
        {
            if (valueBit)
            {
                value |= bytes[++pos] << shift;
            }

            shift += Constants.RefByteShift;
        }

        return value;
    }
}

public record PackMetaInfo(int Version, int PackObjectsCount, string PackSha, byte[] PackObjectBytes);

public record UndeltifiedPackObject(PackObjectType Type, byte[] UncompressedData, string AsciiData);

public record RefDeltaPackObject(
    string BaseObjectSha,
    int SourceSize,
    int TargetSize,
    List<RefDeltaInstruction> RefDeltaInstructions);

public abstract record RefDeltaInstruction;

public record CopyInstruction(int Offset, int Size) : RefDeltaInstruction;

public record InsertInstruction(byte[] BytesToCopyToTarget, string AsciiData) : RefDeltaInstruction;

public enum PackObjectType
{
    Commit = 1,
    Tree = 2,
    Blob = 3,
    Tag = 4,
    OfsDelta = 6,
    RefDelta = 7
}

public static class PackObjectTypeExtensions
{
    public static ObjectType ToObjectType(this PackObjectType packObjectType)
    {
        return packObjectType switch
        {
            PackObjectType.Commit => ObjectType.Commit,
            PackObjectType.Tree => ObjectType.Tree,
            PackObjectType.Blob => ObjectType.Blob,
            PackObjectType.Tag => ObjectType.Tag,
            _ => throw new ArgumentOutOfRangeException(nameof(packObjectType),
                $"Not a valid pack object type: {packObjectType}")
        };
    }
}