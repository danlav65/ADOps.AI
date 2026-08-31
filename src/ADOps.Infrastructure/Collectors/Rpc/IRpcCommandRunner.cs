namespace ADOps.Infrastructure.Collectors.Rpc;

public interface IRpcCommandRunner
{
    Task<string> RunAsync(
        string domainController,
        string target,
        CancellationToken cancellationToken = default);
}