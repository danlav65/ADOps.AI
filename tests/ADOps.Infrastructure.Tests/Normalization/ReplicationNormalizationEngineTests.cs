using ADOps.Core.Entities;
using ADOps.Infrastructure.Evidence;
using ADOps.Infrastructure.Normalization;
using ADOps.Infrastructure.Collectors.Replication;

namespace ADOps.Infrastructure.Tests.Normalization;

public sealed class ReplicationNormalizationEngineTests
{
    [Fact]
    public void Normalize_SuccessfulReplication_CreatesSuccessEvidence()
    {
        var evidenceIdGenerator =
            new FakeEvidenceIdGenerator();

        var engine =
            new ReplicationNormalizationEngine(
                evidenceIdGenerator);

        var record =
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
            };

        var investigationId =
            "INC-SFO-20260709";

        var result =
            engine.Normalize(
                [record],
                investigationId);

        var evidence =
            Assert.Single(result);

        Assert.Equal(
            "EV-000001",
            evidence.EvidenceId);

        Assert.Equal(
            investigationId,
            evidence.InvestigationId);

        Assert.Equal(
            EvidenceType.ReplicationSuccess,
            evidence.Type);

        Assert.Equal(
            "ReplicationCollector",
            evidence.Source);

        Assert.Equal(
            "SFOFLEX-DC1",
            evidence.Target);

        Assert.True(
            evidence.IsValid);
    }

    [Fact]
    public void Normalize_Rpc1722_CreatesRpcFailureEvidence()
    {
        var evidenceIdGenerator =
            new FakeEvidenceIdGenerator();

        var engine =
            new ReplicationNormalizationEngine(
                evidenceIdGenerator);

        var record =
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
                    false,

                ErrorCode =
                    "1722",

                ErrorMessage =
                    "The RPC server is unavailable.",

                CollectedUtc =
                    DateTimeOffset.UtcNow
            };

        var result =
            engine.Normalize(
                [record],
                "INC-SFO-20260709");

        var evidence =
            Assert.Single(result);

        Assert.Equal(
            EvidenceType.RpcFailure,
            evidence.Type);

        Assert.Equal(
            "1722",
            evidence.ErrorCode);

        Assert.Equal(
            "The RPC server is unavailable.",
            evidence.Details);

        Assert.False(
            string.IsNullOrWhiteSpace(
                evidence.EvidenceId));
    }

    [Fact]
    public void Normalize_ReplicationFailure_CreatesFailureEvidence()
    {
        var evidenceIdGenerator =
            new FakeEvidenceIdGenerator();

        var engine =
            new ReplicationNormalizationEngine(
                evidenceIdGenerator);

        var record =
            new ReplicationRecord
            {
                SourceDomainController =
                    "SFOFLEX-DC2",

                DestinationDomainController =
                    "LAX-DC1",

                Partner =
                    "LAX-DC1",

                Site =
                    "SFO",

                Success =
                    false,

                ErrorCode =
                    "8453",

                ErrorMessage =
                    "Replication access was denied.",

                CollectedUtc =
                    DateTimeOffset.UtcNow
            };

        var result =
            engine.Normalize(
                [record],
                "INC-SFO-20260709");

        var evidence =
            Assert.Single(result);

        Assert.Equal(
            EvidenceType.ReplicationFailure,
            evidence.Type);

        Assert.Equal(
            "8453",
            evidence.ErrorCode);

        Assert.Equal(
            "SFOFLEX-DC2",
            evidence.Target);
    }

    [Fact]
    public void Normalize_MultipleRecords_CreatesUniqueEvidenceIds()
    {
        var evidenceIdGenerator =
            new FakeEvidenceIdGenerator();

        var engine =
            new ReplicationNormalizationEngine(
                evidenceIdGenerator);

        var records =
            new[]
            {
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
                        false,

                    ErrorCode =
                        "1722",

                    CollectedUtc =
                        DateTimeOffset.UtcNow
                },

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
                }
            };

        var result =
            engine.Normalize(
                records,
                "INC-SFO-20260709");

        Assert.Equal(
            2,
            result.Count);

        Assert.Equal(
            2,
            result
                .Select(x => x.EvidenceId)
                .Distinct()
                .Count());
    }

    private sealed class FakeEvidenceIdGenerator
        : IEvidenceIdGenerator
    {
        private int _counter;

        public string Generate()
        {
            _counter++;

            return
                $"EV-{_counter:D6}";
        }
    }
}