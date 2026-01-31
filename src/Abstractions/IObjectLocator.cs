using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IObjectLocator
{
    string GetGitObjectFilePath(Repository repo, string objectHash);
}
