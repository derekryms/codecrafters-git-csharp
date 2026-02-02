namespace codecrafters_git;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class GitCommandAttribute(string name) : Attribute
{
    public string Name { get; } = name;
}