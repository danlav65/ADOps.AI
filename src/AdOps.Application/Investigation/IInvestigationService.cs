using ADOps.Application.Reports;
using ADOps.Core.Entities;

namespace ADOps.Application.Investigation;

public interface IInvestigationService
{
    Task<InvestigationReport> InvestigateAsync(
        ADOps.Core.Entities.Investigation investigation,
        CollectorContext context,
        CancellationToken cancellationToken = default);

    Task<InvestigationReport> InvestigateAsync(
        ADOps.Core.Entities.Investigation investigation,
        InvestigationSnapshot snapshot);
}
