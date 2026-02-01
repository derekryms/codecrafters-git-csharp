namespace codecrafters_git.Abstractions;

public interface ICompressionService
{
    byte[] GetDecompressedObject(string objectPath);
    void SaveCompressedObject(string objectPath, byte[] objectBytes);
}