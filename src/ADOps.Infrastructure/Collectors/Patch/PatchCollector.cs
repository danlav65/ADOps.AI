using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Collectors.Patch;

public sealed class PatchCollector
    : IPatchCollector
{
    private readonly IPatchCommandRunner
        _commandRunner;

    public PatchCollector(
        IPatchCommandRunner commandRunner)
    {
        _commandRunner =
            commandRunner;
    }

    public async Task<
        IReadOnlyCollection<PatchRecord>>
        CollectAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
    {
        var records =
            new List<PatchRecord>();

        foreach (var domainController
                 in context.DomainControllers)
        {
            var result =
                await _commandRunner.RunAsync(
                    domainController,
                    cancellationToken);

            records.AddRange(
                result.Records);
        }

        return records;
    }
}