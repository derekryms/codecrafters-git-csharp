namespace codecrafters_git.GitObjects;

public class Tree
{
    public Tree(List<TreeEntry> treeEntries)
    {
        TreeEntries = treeEntries.OrderBy(e => e.Name).ToList();
    }

    public List<TreeEntry> TreeEntries { get; }
}