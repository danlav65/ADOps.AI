using Microsoft.Extensions.DependencyInjection;
using ADOps.Core.Interfaces;
using ADOps.Infrastructure.Collectors.Patch;
using ADOps.Infrastructure.Collectors.Replication;
using ADOps.Infrastructure.Collectors.Rpc;
using ADOps.Infrastructure.Collectors.SystemInfo;
using ADOps.Infrastructure.Correlation;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Ingestion;
using ADOps.Infrastructure.Ingestion.Replication;
using ADOps.Infrastructure.Ingestion.Reporting;
using ADOps.Infrastructure.Ingestion.Patch;
using ADOps.Infrastructure.Ingestion.SystemInfo;
using ADOps.Infrastructure.Ingestion.Rpc;
using ADOps.Infrastructure.Recommendations;
using ADOps.Infrastructure.Analysis;
using ADOps.Infrastructure.Investigation;

namespace ADOps.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services)
    {
        services.AddScoped<
            IReplicationOutputParser,
            ReplicationOutputParser>();

        services.AddScoped<
            IReplicationCollector,
            ReplicationCollector>();

        services.AddScoped<
            IReplicationCommandRunner,
            ReplicationCommandRunner>();

        services.AddScoped<
            IRpcCollector,
            RpcCollector>();

        services.AddScoped<
            IRpcCommandRunner,
            RpcCommandRunner>();

        services.AddScoped<
            IRpcOutputParser,
            RpcOutputParser>();
        
        services.AddScoped<
            IPatchOutputParser,
            PatchOutputParser>();

        services.AddScoped<
            IPatchCollector,
            PatchCollector>();

        services.AddScoped<
            IPatchCommandRunner,
            PatchCommandRunner>();

        services.AddScoped<
            ISystemInfoCollector,
            SystemInfoCollector>();

        services.AddScoped<
            ISystemInfoCommandRunner,
            SystemInfoCommandRunner>();

        services.AddScoped<
            IInvestigationSnapshotBuilder,
            InvestigationSnapshotBuilder>();

        services.AddScoped<
            ICorrelationEngine,
            CorrelationEngine>();

        services.AddScoped<
            IEvidenceIdGenerator,
            EvidenceIdGenerator>();

        services.AddScoped<
            IEvidenceNormalizer,
            EvidenceNormalizer>();

        services.AddScoped<
            IRecommendationEngine,
            RecommendationEngine>();

        services.AddScoped<
            IRootCauseAnalyzer,
            RootCauseAnalyzer>();

        services.AddScoped<
            IReplicationReportIngestor,
            ReplicationReportIngestor>();

        services.AddScoped<
            IPatchReportIngestor,
            PatchReportIngestor>();

        services.AddScoped<
            ISystemInfoReportIngestor,
            SystemInfoReportIngestor>();

        services.AddScoped<
            IRpcReportIngestor,
            RpcReportIngestor>();

        // Ingestion

        services.AddSingleton<ReportBundleValidator>();
        services.AddScoped<
            IReportBundleIngestionService, 
            ReportBundleIngestionService>();

        return services;
    }
}
