using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

namespace ADOps.Infrastructure.Collectors.SystemInfo;

public sealed class SystemInfoCollector
    : ISystemInfoCollector
{
    private readonly ISystemInfoCommandRunner
        _commandRunner;

    public SystemInfoCollector(
        ISystemInfoCommandRunner commandRunner)
    {
        _commandRunner =
            commandRunner;
    }

    public async Task<IReadOnlyCollection<SystemInfoRecord>>
        CollectAsync(
            CollectorContext context,
            CancellationToken cancellationToken = default)
    {
        var records =
            new List<SystemInfoRecord>();

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