using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

public interface ISystemInfoCollector
{
    Task<IReadOnlyCollection<SystemInfoRecord>> CollectAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default);
}