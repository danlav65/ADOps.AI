using ADOps.Core.Entities;
using EvidenceEntity = ADOps.Core.Entities.Evidence;

namespace ADOps.Infrastructure.Evidence;

public interface IEvidenceNormalizer
{
    IReadOnlyCollection<EvidenceEntity> Normalize(
        InvestigationSnapshot snapshot);
}