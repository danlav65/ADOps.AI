using ADOps.Core.Entities;

namespace ADOps.Core.Interfaces;

public interface IInvestigationSnapshotBuilder
{
    Task<InvestigationSnapshot> BuildAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default);
}