using ADOps.Application.Presentation;
using ADOps.Application.Reports;
using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Application.Investigation;

public sealed class InvestigationService : IInvestigationService
{
    private readonly IInvestigationSnapshotBuilder _snapshotBuilder;
    private readonly ICorrelationEngine _correlationEngine;
    private readonly IRootCauseAnalyzer _rootCauseAnalyzer;
    private readonly IRecommendationEngine _recommendationEngine;
    private readonly InvestigationPresenter _presenter;

    public InvestigationService(
        IInvestigationSnapshotBuilder snapshotBuilder,
        ICorrelationEngine correlationEngine,
        IRootCauseAnalyzer rootCauseAnalyzer,
        IRecommendationEngine recommendationEngine,
        InvestigationPresenter presenter)
    {
        _snapshotBuilder =
            snapshotBuilder ??
            throw new ArgumentNullException(nameof(snapshotBuilder));

        _correlationEngine =
            correlationEngine ??
            throw new ArgumentNullException(nameof(correlationEngine));

        _rootCauseAnalyzer =
            rootCauseAnalyzer ??
            throw new ArgumentNullException(nameof(rootCauseAnalyzer));

        _recommendationEngine =
            recommendationEngine ??
            throw new ArgumentNullException(nameof(recommendationEngine));

        _presenter =
            presenter ??
            throw new ArgumentNullException(nameof(presenter));
    }

    public async Task<InvestigationReport> InvestigateAsync(
        ADOps.Core.Entities.Investigation investigation,
        CollectorContext context,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        ArgumentNullException.ThrowIfNull(context);

        var snapshot =
            await _snapshotBuilder.BuildAsync(
                context,
                cancellationToken);

        return Investigate(
            investigation,
            snapshot);
    }

    public Task<InvestigationReport> InvestigateAsync(
        ADOps.Core.Entities.Investigation investigation,
        InvestigationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        ArgumentNullException.ThrowIfNull(snapshot);

        return Task.FromResult(
            Investigate(
                investigation,
                snapshot));
    }

    private InvestigationReport Investigate(
        ADOps.Core.Entities.Investigation investigation,
        InvestigationSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(investigation);
        ArgumentNullException.ThrowIfNull(snapshot);

        if (snapshot.Topology is null)
        {
            throw new InvalidOperationException(
                "Investigation snapshot does not contain topology information.");
        }

        var findings =
            _correlationEngine.Correlate(
                snapshot.Evidence,
                snapshot.Topology);

        var rootCauseAnalysis =
            _rootCauseAnalyzer.Analyze(findings);

        var recommendations =
            _recommendationEngine.Generate(
                rootCauseAnalysis,
                findings);

        return _presenter.Build(
            investigation,
            snapshot.Evidence,
            findings,
            rootCauseAnalysis,
            recommendations);
    }
}
