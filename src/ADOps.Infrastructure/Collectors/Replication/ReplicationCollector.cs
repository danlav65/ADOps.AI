using ADOps.Core.Entities;
using ADOps.Core.Entities.Replication;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Collectors.Replication;

public sealed class ReplicationCollector
    : IReplicationCollector
{
    private readonly IReplicationCommandRunner _commandRunner;
    private readonly IReplicationOutputParser _parser;

    public ReplicationCollector(
        IReplicationCommandRunner commandRunner,
        IReplicationOutputParser parser)
    {
        _commandRunner = commandRunner;
        _parser = parser;
    }

    public string Name => "ReplicationCollector";

    public async Task<IReadOnlyCollection<ReplicationRecord>> CollectAsync(
        CollectorContext context,
        CancellationToken cancellationToken = default)
    {
        var records = new List<ReplicationRecord>();

        foreach (var domainController in context.DomainControllers)
        {
            try
            {
                var commandResult =
                    await _commandRunner.RunAsync(
                        domainController,
                        cancellationToken);

                var parsedRecords =
                    _parser.Parse(
                        domainController,
                        commandResult,
                        context);

                records.AddRange(parsedRecords);
            }
            catch
            {
                // Collector failures are handled by the command runner
                // and investigation pipeline. Continue collecting from
                // remaining domain controllers.
            }
        }

        return records;
    }
}
