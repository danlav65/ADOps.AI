namespace ADOps.Infrastructure.Collectors.SystemInfo;

public interface ISystemInfoCommandRunner
{
    Task<SystemInfoCommandResult> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default);
}
