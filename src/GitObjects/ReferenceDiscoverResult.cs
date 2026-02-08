using codecrafters_git.Implementations;

namespace codecrafters_git.GitObjects;

public record ReferenceDiscoverResult(string HeadHash, List<Reference> References);