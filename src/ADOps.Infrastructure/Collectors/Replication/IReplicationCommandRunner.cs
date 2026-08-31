namespace ADOps.Infrastructure.Collectors.Replication;

public interface IReplicationCommandRunner
{
    Task<string> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default);
}