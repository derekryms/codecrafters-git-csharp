using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class GitHttpClient(IHttpClientFactory httpClientFactory, IFileSystem fileSystem) : IGitClient
{
    private const string ReferenceDiscoveryQuery = "info/refs?service=git-upload-pack";
    
    public async Task<ReferenceDiscoverResult> DiscoverReferences(string repoToCloneUrl)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var referenceDiscoverUrl = fileSystem.Combine(repoToCloneUrl, ReferenceDiscoveryQuery);
        var response = await httpClient.GetAsync(referenceDiscoverUrl);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception(
                $"Failed to discover references for {repoToCloneUrl}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var refs = result.Split('\n').Skip(1).SkipLast(1).ToList();
        var headHash = refs.First().Split(' ').First()[^Constants.ShaHexStringLength..];
        var references = refs.Skip(1).Select(r =>
        {
            var parts = r.Split(' ');
            var hash = parts[0][^Constants.ShaHexStringLength..];
            var name = parts[1];
            return new Reference(hash, name);
        }).ToList();

        return new ReferenceDiscoverResult(headHash, references);
    }
}