namespace codecrafters_git.Abstractions;

public interface ICompressionService
{
    byte[] GetDecompressedObject(string objectPath);
}