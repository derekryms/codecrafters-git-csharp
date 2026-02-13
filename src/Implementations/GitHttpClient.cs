using System.Net.Http.Headers;
using codecrafters_git.Abstractions;
using codecrafters_git.GitObjects;

namespace codecrafters_git.Implementations;

public class GitHttpClient(IHttpClientFactory httpClientFactory, IFileSystem fileSystem) : IGitClient
{
    private const string ReferenceDiscoveryQuery = "info/refs?service=git-upload-pack";
    private const string PackNegotiationQuery = "git-upload-pack";
    private const string PackNegotiationHeaderContentType = "application/x-git-upload-pack-request";

    public async Task<ReferenceDiscoverResult> DiscoverReferences(string repoToCloneUrl)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var referenceDiscoverUrl = fileSystem.Combine(repoToCloneUrl, ReferenceDiscoveryQuery);
        using var response = await httpClient.GetAsync(referenceDiscoverUrl);
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

    public async Task<Pack> NegotiatePack(string repoToCloneUrl, string sha)
    {
        using var httpClient = httpClientFactory.CreateClient();
        var packNegotiationUrl = fileSystem.Combine(repoToCloneUrl, PackNegotiationQuery);
        using var packNegotiationRequest = GeneratePackNegotiationRequest(sha);
        using var response = await httpClient.PostAsync(packNegotiationUrl, packNegotiationRequest);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to negotiate pack for {repoToCloneUrl}. Status code: {response.StatusCode}");
        }

        var result = await response.Content.ReadAsStringAsync();
        return new Pack { Content = result };
    }

    private static StringContent GeneratePackNegotiationRequest(string sha)
    {
        var want = GeneratePktLine($"want {sha}\n").Line;
        const string end = "0000";
        var done = GeneratePktLine("done\n").Line;
        var content = new StringContent(want + end + done);
        content.Headers.ContentType = new MediaTypeHeaderValue(PackNegotiationHeaderContentType);
        return content;
    }

    private static PktLine GeneratePktLine(string content)
    {
        return new PktLine($"{content.Length + 4:X4}{content}");
    }
}

public record PktLine(string Line);

public class Pack
{
    public string Content { get; set; }
}