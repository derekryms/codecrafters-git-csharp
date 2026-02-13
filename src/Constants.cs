namespace codecrafters_git;

public static class Constants
{
    public const byte NullByte = 0;
    public const byte SpaceByte = 32;
    public const int ShaByteLength = 20;
    public const int ShaHexStringLength = 40;

    public const byte MsbMask = 0x80; // 0b10000000
    public const byte FirstByteTypeMask = 0x70; // 0b01110000
    public const byte FirstByteSizeShift = 4;
    public const byte FirstByteSizeMask = 0x0F; // 0b00001111
    public const byte NByteSizeMask = 0x7F; // 0b01111111
    public const byte NByteSizeShift = 7;

    public const byte ByteSize3Mask = 0x40; // 0b01000000
    public const byte ByteSize2Mask = 0x20; // 0b00100000
    public const byte ByteSize1Mask = 0x10; // 0b00010000
    public const byte ByteOffset4Mask = 0x08; // 0b00001000
    public const byte ByteOffset3Mask = 0x04; // 0b00000100
    public const byte ByteOffset2Mask = 0x02; // 0b00000010
    public const byte ByteOffset1Mask = 0x01; // 0b00000001
    public const byte RefByteShift = 8;

    public const byte RefByteInsertSizeMask = 0x7F; // 0b01111111

    public static readonly byte[] PackHeader = "PACK"u8.ToArray();
}