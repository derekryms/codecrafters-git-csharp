using codecrafters_git.GitObjects;

namespace codecrafters_git.Abstractions;

public interface IGitClient
{
    Task<ReferenceDiscoverResult> DiscoverReferences(string repoToCloneUrl);
}