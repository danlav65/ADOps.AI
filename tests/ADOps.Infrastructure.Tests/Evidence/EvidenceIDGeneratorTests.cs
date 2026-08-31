using ADOps.Infrastructure.Evidence;

namespace ADOps.Infrastructure.Tests.Evidence;

public sealed class EvidenceIdGeneratorTests
{
    [Fact]
    public void Generate_ReturnsSequentialEvidenceIds()
    {
        var generator =
            new EvidenceIdGenerator();

        var first =
            generator.Generate();

        var second =
            generator.Generate();

        var third =
            generator.Generate();

        Assert.Equal(
            "EV-000001",
            first);

        Assert.Equal(
            "EV-000002",
            second);

        Assert.Equal(
            "EV-000003",
            third);
    }

    [Fact]
    public async Task Generate_IsThreadSafe()
    {
        var generator =
            new EvidenceIdGenerator();

        var ids =
            await Task.WhenAll(
                Enumerable.Range(1, 100)
                    .Select(_ =>
                        Task.Run(
                            () =>
                                generator.Generate())));

        Assert.Equal(
            100,
            ids.Distinct().Count());
    }
}