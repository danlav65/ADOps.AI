using ADOps.Core.Entities;
using ADOps.Infrastructure.Knowledge;

namespace ADOps.Infrastructure.Tests.Knowledge;

public sealed class InMemoryKnowledgeRetrieverTests
{
    private readonly InMemoryKnowledgeRetriever _retriever = new();

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
