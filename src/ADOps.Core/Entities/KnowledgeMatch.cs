namespace ADOps.Core.Entities;

public sealed class KnowledgeMatch : EntityBase
{
    public required string Source { get; init; }

    public required string Description { get; init; }

    public double SimilarityScore { get; private set; }

    public Guid? RelatedInvestigationId { get; init; }

    public void SetSimilarity(double score)
    {
        SimilarityScore = score;
        Touch();
    }
}