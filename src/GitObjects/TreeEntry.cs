namespace codecrafters_git.GitObjects;

public record TreeEntry(TreeEntryMode Mode, string Hash, string Name)
{
    public ObjectType Type => Mode == TreeEntryMode.Directory ? ObjectType.Tree : ObjectType.Blob;

    public string ModeValue => Mode switch
    {
        TreeEntryMode.RegularFile => "100644",
        TreeEntryMode.ExecutableFile => "100755",
        TreeEntryMode.SymbolicLink => "120000",
        TreeEntryMode.Directory => "40000",
        _ => throw new ArgumentOutOfRangeException()
    };
}