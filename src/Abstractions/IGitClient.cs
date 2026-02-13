using codecrafters_git.GitObjects;
using codecrafters_git.Implementations;

namespace codecrafters_git.Abstractions;

public interface IGitClient
{
    Task<ReferenceDiscoverResult> DiscoverReferences(string repoToCloneUrl);
    Task<Pack> NegotiatePack(string repoToCloneUrl, string sha);
}