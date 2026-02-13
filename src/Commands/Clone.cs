using codecrafters_git.Abstractions;

namespace codecrafters_git.Commands;

[GitCommand("clone")]
public class Clone(
    IHttpClientFactory factory,
    IGitClient gitClient,
    IRepositoryFactory repoFactory,
    IFileSystem fileSystem,
    IOutputWriter output) : ICommand
{
    public void Execute(string[] args)
    {
        switch (args.Length)
        {
            case not 2:
                output.WriteLine("Usage: clone <repo> <dir>");
                return;
        }

        new Init(repoFactory, fileSystem, output).Execute([args[1]]);
        var referenceDiscoverResult = gitClient.DiscoverReferences(args[0]).Result;
        var pack = gitClient.NegotiatePack(args[0], referenceDiscoverResult.HeadHash).Result;
    }
}