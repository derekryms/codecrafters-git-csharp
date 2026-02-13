using codecrafters_git.Implementations;

namespace codecrafters_git.Abstractions;

public interface IPackParser
{
    Pack ParsePackBytes(byte[] negotiationBytes);
}