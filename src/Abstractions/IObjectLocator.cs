using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IObjectLocator
{
    string GetGitObjectFilePath(Repository repo, string objectHash);
    string CreateGitObjectDirectory(Repository repo, string objectHash);
    GitObjectPath ComputeGitObjectFilePath(Repository repo, string objectHash);
}