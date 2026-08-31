using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

public interface IRpcCollector
{
    Task<IReadOnlyCollection<RpcRecord>> CollectAsync(
        CollectorContext context,
        TopologyContext topology,
        CancellationToken cancellationToken = default);
}