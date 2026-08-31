using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

public interface IPatchCollector
{
    Task<IReadOnlyCollection<PatchRecord>> CollectAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default);
}