using ADOps.Infrastructure.Collectors;
using ADOps.Infrastructure.Collectors.Replication;

namespace ADOps.Infrastructure.Tests.Collectors.Replication;

public sealed class ReplicationCollectorTests
{
    [Fact]
    public async Task CollectAsync_ReturnsSuccessfulRecords()
    {
        var commandRunner =
            new FakeReplicationCommandRunner(
                new ReplicationCommandResult
                {
                    TargetDomainController =
                        "SFOFLEX-DC1",

                    StandardOutput =
                        "Replication succeeded.",

                    StandardError =
                        string.Empty,

                    ExitCode =
                        0,

                    Duration =
                        TimeSpan.FromSeconds(1),

                    ExecutedUtc =
                        DateTimeOffset.UtcNow
                });

        var parser =
            new FakeReplicationOutputParser(
                new ReplicationRecord
                {
                    SourceDomainController =
                        "SFOFLEX-DC1",

                    DestinationDomainController =
                        "ZUSW-DC1",

                    Partner =
                        "ZUSW-DC1",

                    Site =
                        "SFO",

                    Success =
                        true,

                    CollectedUtc =
                        DateTimeOffset.UtcNow
                });

        var collector =
            new ReplicationCollector(
                commandRunner,
                parser);

        var context =
            CreateContext(
                "SFOFLEX-DC1");

        var result =
            await collector.CollectAsync(
                context);

        Assert.True(
            result.Success);

        Assert.Single(
            result.Data);

        Assert.Equal(
            "SFOFLEX-DC1",
            result.Data.First()
                .SourceDomainController);

        Assert.True(
            result.Data.First()
                .Success);
    }

    [Fact]
    public async Task CollectAsync_PreservesCollectorErrors()
    {
        var commandRunner =
            new ThrowingReplicationCommandRunner(
                "RPC connection failed.");

        var parser =
            new FakeReplicationOutputParser();

        var collector =
            new ReplicationCollector(
                commandRunner,
                parser);

        var context =
            CreateContext(
                "SFOFLEX-DC1");

        var result =
            await collector.CollectAsync(
                context);

        Assert.False(
            result.Success);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "RPC connection failed."));
    }

    [Fact]
    public async Task CollectAsync_ContinuesWhenOneDomainControllerFails()
    {
        var commandRunner =
            new MultiTargetReplicationCommandRunner();

        var parser =
            new FakeReplicationOutputParser(
                new ReplicationRecord
                {
                    SourceDomainController =
                        "SFOFLEX-DC2",

                    DestinationDomainController =
                        "ZUSW-DC1",

                    Partner =
                        "ZUSW-DC1",

                    Site =
                        "SFO",

                    Success =
                        true,

                    CollectedUtc =
                        DateTimeOffset.UtcNow
                });

        var collector =
            new ReplicationCollector(
                commandRunner,
                parser);

        var context =
            CreateContext(
                "SFOFLEX-DC1",
                "SFOFLEX-DC2");

        var result =
            await collector.CollectAsync(
                context);

        Assert.False(
            result.Success);

        Assert.Single(
            result.Data);

        Assert.Contains(
            result.Errors,
            error =>
                error.Contains(
                    "SFOFLEX-DC1"));
    }

    private static CollectorContext CreateContext(
        params string[] domainControllers)
    {
        return new CollectorContext
        {
            DomainControllers =
                domainControllers,

            Site =
                "SFO"
        };
    }

    private sealed class FakeReplicationCommandRunner
        : IReplicationCommandRunner
    {
        private readonly ReplicationCommandResult _result;

        public FakeReplicationCommandRunner(
            ReplicationCommandResult result)
        {
            _result = result;
        }

        public Task<ReplicationCommandResult> RunAsync(
            string domainController,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                _result);
        }
    }

    private sealed class ThrowingReplicationCommandRunner
        : IReplicationCommandRunner
    {
        private readonly string _message;

        public ThrowingReplicationCommandRunner(
            string message)
        {
            _message = message;
        }

        public Task<ReplicationCommandResult> RunAsync(
            string domainController,
            CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException(
                _message);
        }
    }

    private sealed class MultiTargetReplicationCommandRunner
        : IReplicationCommandRunner
    {
        public Task<ReplicationCommandResult> RunAsync(
            string domainController,
            CancellationToken cancellationToken = default)
        {
            if (domainController ==
                "SFOFLEX-DC1")
            {
                throw new InvalidOperationException(
                    "RPC connection failed.");
            }

            return Task.FromResult(
                new ReplicationCommandResult
                {
                    TargetDomainController =
                        domainController,

                    StandardOutput =
                        "Replication succeeded.",

                    StandardError =
                        string.Empty,

                    ExitCode =
                        0,

                    Duration =
                        TimeSpan.FromSeconds(1),

                    ExecutedUtc =
                        DateTimeOffset.UtcNow
                });
        }
    }

    private sealed class FakeReplicationOutputParser
        : IReplicationOutputParser
    {
        private readonly ReplicationRecord[] _records;

        public FakeReplicationOutputParser(
            params ReplicationRecord[] records)
        {
            _records =
                records;
        }

        public IReadOnlyCollection<ReplicationRecord> Parse(
            ReplicationCommandResult commandResult,
            CollectorContext context)
        {
            return _records;
        }
    }
}