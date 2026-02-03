using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public record GitObject(ObjectHeader Header, byte[] Content);