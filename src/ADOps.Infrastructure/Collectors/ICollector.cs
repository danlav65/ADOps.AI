using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors;

public interface ICollector<T>
{
    string Name { get; }

    Task<CollectorResult<T>> CollectAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default);
}
