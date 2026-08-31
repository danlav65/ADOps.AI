using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;

namespace ADOps.Infrastructure.Collectors.Replication;

public interface IReplicationOutputParser
{
    IReadOnlyCollection<ReplicationRecord> Parse(
        string sourceDomainController,
        string commandOutput,
        CollectorContext context);
}