using System.IO.Pipelines;
using ADOps.Application.Knowledge;
using ADOps.Core.Entities;
using ADOps.Core.Interfaces;
using Microsoft.VisualStudio.TestPlatform.Common.DataCollection;

namespace ADOps.Application.Tests;

public sealed class KnowledgeServiceTests
{
    [Fact]
    public void Constructor_Throws_WhenRetrieverIsNull()
    {
        Assert.Throws<ArgumentNullException>(
            () => new KnowledgeService(null!));
    }

    [Fact]
    public void RetrieveWithContext_ReturnsMatchesAndMarksAnalysisNotPerformed()
    {
        var query = new KnowledgeQuery
        {
            Query = "replication"
        };

        var match = new KnowledgeMatch
        {
            Source = "Test Knowledge",
            Description = "Replication guidance.",
            Provenance = new KnowledgeSource
            {
                SourceId = "fixture-replication",
                Title = "Replication Test Guidance",
                Publisher = "ADOps AI Test Fixture",
                RetrievedUtc = new DateTimeOffset(
                    2026, 1, 1, 0, 0, 0, TimeSpan.Zero)
            }
        };

        var retriever = new FakeKnowledgeRetriever
        {
            Result = [match]
        };

        var service = new KnowledgeService(retriever);

        var result = service.RetrieveWithContext(query);

        Assert.Same(match, Assert.Single(result.Matches));
        Assert.Same(
            match.Provenance,
            Assert.Single(result.Matches).Provenance);

        Assert.Empty(result.Conflicts);

        Assert.Equal(
            ADOps.Core.Enums.KnowledgeConflictAnalysisStatus.NotPerformed,
            result.ConflictAnalysisStatus);

        Assert.Same(query, retriever.ReceivedQuery);
    }

    [Fact]
    public void RetrieveWithContext_ReturnsEmptyMatches_WhenNoneFound()
    {
        var service = new KnowledgeService(
            new FakeKnowledgeRetriever());

        var result = service.RetrieveWithContext(
            new KnowledgeQuery
            {
                Query = "unknown"
            });

        Assert.Empty(result.Matches);
        Assert.Empty(result.Conflicts);
    }

    [Fact]
    public void RetrieveWithContext_Throws_WhenQueryIsNull()
    {
        var service = new KnowledgeService(
            new FakeKnowledgeRetriever());

        Assert.Throws<ArgumentNullException>(
            () => service.RetrieveWithContext(null!));
    }

    [Fact]
    public void Retrieve_DelegatesQueryToRetriever()
    {
        var query =
            new KnowledgeQuery
            {
                Query = "replication",
                Site = "SFO",
                MaxResults = 3
            };

        var retriever =
            new FakeKnowledgeRetriever();

        var service =
            new KnowledgeService(retriever);

        service.Retrieve(query);

        Assert.Same(
            query,
            retriever.ReceivedQuery);
    }

    [Fact]
    public void Retrieve_ReturnsMatchesFromRetriever()
    {
        var query =
            new KnowledgeQuery
            {
                Query = "replication"
            };

        var match =
            new KnowledgeMatch
            {
                Source = "Test Knowledge",
                Description = "Replication test guidance."
            };

        var retriever =
            new FakeKnowledgeRetriever
            {
                Result = [match]
            };

        var service =
            new KnowledgeService(retriever);

        var result =
            service.Retrieve(query);

        var returnedMatch =
            Assert.Single(result);

        Assert.Same(
            match,
            returnedMatch);
    }

    [Fact]
    public void Retrieve_ReturnsEmpty_WhenRetrieverReturnsNoMatches()
    {
        var query =
            new KnowledgeQuery
            {
                Query = "unknown"
            };

        var retriever =
            new FakeKnowledgeRetriever
            {
                Result = []
            };

        var service =
            new KnowledgeService(retriever);

        var result =
            service.Retrieve(query);

        Assert.Empty(result);
    }

    [Fact]
    public void Retrieve_Throws_WhenQueryIsNull()
    {
        var retriever =
            new FakeKnowledgeRetriever();

        var service =
            new KnowledgeService(retriever);

        Assert.Throws<ArgumentNullException>(
            () => service.Retrieve(null!));
    }

    private sealed class FakeKnowledgeRetriever : IKnowledgeRetriever
    {
        public KnowledgeQuery? ReceivedQuery { get; private set; }

        public IReadOnlyCollection<KnowledgeMatch> Result { get; init; } =
            [];

        public IReadOnlyCollection<KnowledgeMatch> Retrieve(
            KnowledgeQuery query)
        {
            ReceivedQuery = query;

            return Result;
        }
    }
}
