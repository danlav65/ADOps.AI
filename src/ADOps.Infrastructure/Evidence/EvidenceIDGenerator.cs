using System.Threading;

namespace ADOps.Infrastructure.Evidence;

public sealed class EvidenceIdGenerator
    : IEvidenceIdGenerator
{
    private long _counter;

    public string Generate()
    {
        var value =
            Interlocked.Increment(
                ref _counter);

        return $"EV-{value:D6}";
    }
}