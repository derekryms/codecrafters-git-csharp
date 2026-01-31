namespace codecrafters_git.GitObjects;

public record struct ObjectHeader(ObjectType Type, int Length);