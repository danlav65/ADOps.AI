using ADOps.Core.Enums;
using ADOps.Core.Entities;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class InMemoryKnowledgeRetrieverTests
{
    private readonly InMemoryKnowledgeRetriever _retriever = new();

    [Fact]
    public void Retrieve_ReturnsProvenanceForEveryMatch()
    {
        var results = _retriever.Retrieve(
            new KnowledgeQuery
            {
                Query = "replication"
            });

        Assert.NotEmpty(results);

        Assert.All(results, match =>
        {
            var provenance = Assert.IsType<KnowledgeSource>(
                match.Provenance);

            Assert.False(
                string.IsNullOrWhiteSpace(provenance.SourceId));

            Assert.Equal(
                "ADOps AI Test Fixture",
                provenance.Publisher);

            Assert.Equal(
                KnowledgeSourceType.Unknown,
                provenance.SourceType);

            Assert.Null(provenance.SourceUri);
        });
    }

    [Fact]
    public void Retrieve_PreservesDistinctFixtureSourceIds()
    {
        var results = _retriever.Retrieve(
            new KnowledgeQuery
            {
                Query = "replication",
                MaxResults = 10
            });

        var sourceIds = results
            .Select(match => match.Provenance!.SourceId)
            .ToArray();

        Assert.NotEmpty(sourceIds);

        Assert.Equal(
            sourceIds.Length,
            sourceIds.Distinct().Count());
    }

    [Fact]
    public void Retrieve_ReturnsMatchingKnowledge()
    {
        var query = new KnowledgeQuery
        {
            Query = "replication DNS"
        };

        var results = _retriever.Retrieve(query);

        Assert.NotEmpty(results);
        Assert.Contains(
            results,
            result => result.Source == "SITA AD Architecture");
    }

    [Fact]
    public void Retrieve_IsCaseInsensitive()
    {
        var query = new KnowledgeQuery
        {
            Query = "KERBEROS"
        };

        var results = _retriever.Retrieve(query);

        Assert.NotEmpty(results);
        Assert.Contains(
            results,
            result => result.Source == "SITA AD Operations");
    }

    [Fact]
    public void Retrieve_RespectsMaximumResults()
    {
        var query = new KnowledgeQuery
        {
            Query = "AD",
            MaxResults = 1
        };

        var results = _retriever.Retrieve(query);

        Assert.Single(results);
    }

    [Fact]
    public void Retrieve_ReturnsEmptyForBlankQuery()
    {
        var query = new KnowledgeQuery
        {
            Query = " "
        };

        var results = _retriever.Retrieve(query);

        Assert.Empty(results);
    }

    [Fact]
    public void Retrieve_ThrowsForNullQuery()
    {
        Assert.Throws<ArgumentNullException>(
            () => _retriever.Retrieve(null!));
    }
}
