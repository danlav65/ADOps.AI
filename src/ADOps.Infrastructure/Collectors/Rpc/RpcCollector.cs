using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Collectors.Rpc;

public sealed class RpcCollector : IRpcCollector
{
    private readonly IRpcCommandRunner _commandRunner;
    private readonly IRpcOutputParser _parser;

    public RpcCollector(
        IRpcCommandRunner commandRunner,
        IRpcOutputParser parser)
    {
        _commandRunner = commandRunner;
        _parser = parser;
    }

    public async Task<IReadOnlyCollection<RpcRecord>> CollectAsync(
        CollectorContext context,
        TopologyContext topology,
        CancellationToken cancellationToken = default)
    {
        var records = new List<RpcRecord>();

        foreach (var relationship in topology.ReplicationPartners)
        {
            if (!relationship.IsActive)
            {
                continue;
            }

            try
            {
                var commandResult =
                    await _commandRunner.RunAsync(
                        relationship.SourceDomainController,
                        relationship.PartnerDomainController,
                        cancellationToken);

                var record =
                    _parser.Parse(
                        relationship.SourceDomainController,
                        commandResult,
                        context);

                records.Add(record);
            }
            catch
            {
                // Continue testing remaining topology relationships.
            }
        }

        return records;
    }
}