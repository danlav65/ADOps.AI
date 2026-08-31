using ADOps.Core.Entities;

namespace ADOps.Infrastructure.Collectors.Patch;

public interface IPatchOutputParser
{
    IReadOnlyCollection<PatchRecord> Parse(
        string domainController,
        string commandOutput,
        DateTimeOffset collectedUtc);
}
