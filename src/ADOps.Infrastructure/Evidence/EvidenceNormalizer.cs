using ADOps.Core.Entities;
using EvidenceEntity = ADOps.Core.Entities.Evidence;

namespace ADOps.Infrastructure.Evidence;

public sealed class EvidenceNormalizer
    : IEvidenceNormalizer
{
    private readonly IEvidenceIdGenerator _evidenceIdGenerator;

    public EvidenceNormalizer(
        IEvidenceIdGenerator evidenceIdGenerator)
    {
        _evidenceIdGenerator = evidenceIdGenerator;
    }

    public IReadOnlyCollection<EvidenceEntity> Normalize(
        InvestigationSnapshot snapshot)
    {
        var evidence = new List<EvidenceEntity>();

        foreach (var record in snapshot.Replication)
        {
            evidence.Add(
                NormalizeReplication(
                    snapshot.InvestigationId,
                    record));
        }

        foreach (var record in snapshot.Patches)
        {
            evidence.Add(
                NormalizePatch(
                    snapshot.InvestigationId,
                    record));
        }

        foreach (var record in snapshot.Rpc)
        {
            if (!record.Success)
            {
                evidence.Add(
                NormalizeRpc(
                    snapshot.InvestigationId,
                    record));
            }
        }
        
        foreach (var record in snapshot.SystemInfo)
        {
            evidence.Add(
                NormalizeSystemInfo(
                    snapshot.InvestigationId,
                    record));
        }

        return evidence;
    }

    private EvidenceEntity NormalizeReplication(
        string investigationId,
        ADOps.Core.Entities.Replication.ReplicationRecord record)
    {
        var evidenceType =
            record.Success
                ? EvidenceType.ReplicationSuccess
                : EvidenceType.ReplicationFailure;

        var summary =
            record.Success
                ? $"Replication succeeded from {record.SourceDomainController} to {record.PartnerDomainController}."
                : $"Replication failed from {record.SourceDomainController} to {record.PartnerDomainController}.";

        return new EvidenceEntity
        {
            EvidenceId = _evidenceIdGenerator.Generate(),
            InvestigationId = investigationId,
            Type = evidenceType,
            Source = record.SourceDomainController,
            Target = record.PartnerDomainController,
            CollectedUtc = record.CollectedUtc,
            Summary = summary,
            ErrorCode = record.ErrorCode?.ToString(),
            Details = record.ErrorMessage,
            IsValid = true
        };
    }

   private EvidenceEntity NormalizeRpc(
    string investigationId,
    RpcRecord record)
{
    return new EvidenceEntity
    {
        EvidenceId = _evidenceIdGenerator.Generate(),
        InvestigationId = investigationId,
        Type = EvidenceType.RpcFailure,
        Source = record.DomainController,
        Target = record.Target,
        CollectedUtc = record.CollectedUtc,
        Summary =
            $"RPC connectivity failed from {record.DomainController} to {record.Target}.",
        ErrorCode = record.ErrorCode?.ToString(),
        Details = record.ErrorMessage,
        IsValid = true
    };
} 
    
    private EvidenceEntity NormalizePatch(
        string investigationId,
        PatchRecord record)
    {
        var summary =
            record.Installed
                ? $"Patch {record.KnowledgeBaseArticle} is installed on {record.DomainController}."
                : $"Patch {record.KnowledgeBaseArticle} is not installed on {record.DomainController}.";

        return new EvidenceEntity
        {
            EvidenceId = _evidenceIdGenerator.Generate(),
            InvestigationId = investigationId,
            Type = EvidenceType.Patch,
            Source = "PatchCollector",
            Target = record.DomainController,
            CollectedUtc = record.CollectedUtc,
            Summary = summary,
            Details =
                $"Site: {record.Site}; " +
                $"Operating System: {record.OperatingSystem}; " +
                $"OS Build: {record.OsBuild}; " +
                $"Patch Version: {record.PatchVersion}; " +
                $"Installed UTC: {record.InstalledUtc?.ToString("O") ?? "Unknown"}",
            IsValid = true
        };
    }

    private EvidenceEntity NormalizeSystemInfo(
        string investigationId,
        SystemInfoRecord record)
    {
        var summary =
            $"System health information collected from {record.DomainController}.";

        return new EvidenceEntity
        {
            EvidenceId = _evidenceIdGenerator.Generate(),
            InvestigationId = investigationId,
            Type = EvidenceType.InfrastructureHealth,
            Source = "SystemInfoCollector",
            Target = record.DomainController,
            CollectedUtc = record.CollectedUtc,
            Summary = summary,
            Details =
                $"Site: {record.Site}; " +
                $"Computer: {record.ComputerName}; " +
                $"Operating System: {record.OperatingSystem}; " +
                $"OS Version: {record.OsVersion}; " +
                $"Build: {record.BuildNumber}; " +
                $"Edition: {record.Edition}; " +
                $"Architecture: {record.Architecture}; " +
                $"Logical Processors: {record.LogicalProcessors}; " +
                $"Memory GB: {record.PhysicalMemoryGb}; " +
                $"System Drive Free Space GB: {record.SystemDriveFreeSpaceGb}; " +
                $"Virtual Machine: {record.VirtualMachine}; " +
                $"Hypervisor: {record.Hypervisor ?? "Unknown"}",
            IsValid = true
        };
    }
}
