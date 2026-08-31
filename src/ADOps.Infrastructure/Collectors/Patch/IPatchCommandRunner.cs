using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.Patch;

public interface IPatchCommandRunner
{
    Task<PatchCommandResult> RunAsync(
        string domainController,
        CancellationToken cancellationToken = default);
}
