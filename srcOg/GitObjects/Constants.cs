namespace codecrafters_git.srcOg.GitObjects;

public static class Constants
{
    // pack object's header constants
    public const byte MSBMask = 0x80;
    public const byte ObjectSizeFirstByteMask = 0x0F;
    public const byte ObjectSizeNthByteMask = 0x7F;
    public const byte ObjectTypeByteMask = 0x07;
    public const int ObjectSizeFirstByteShift = 4;
    public const int ObjectSizeNthByteShift = 7;
}