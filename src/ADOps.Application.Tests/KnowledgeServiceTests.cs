using ADOps.Application.Knowledge;
using ADOps.Core.Entities;
using ADOps.Core.Interfaces;

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
