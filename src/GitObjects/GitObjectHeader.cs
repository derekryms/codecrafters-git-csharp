namespace codecrafters_git.GitObjects;

public record struct GitObjectHeader(GitObjectType Type, int Length);