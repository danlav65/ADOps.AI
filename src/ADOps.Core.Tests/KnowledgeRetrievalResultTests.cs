using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Core.Tests;

public sealed class KnowledgeRetrievalResultTests
{
    [Fact]
    public void KnowledgeRetrievalResult_PreservesMatchesAndConflicts()
    {
        var retrievedUtc = new DateTimeOffset(
            2026, 9, 19, 12, 0, 0, TimeSpan.Zero);

        var match = new KnowledgeMatch
        {
            Source = "Microsoft Support",
            Description = "Case-specific guidance."
        };

        var conflict = new KnowledgeConflict
        {
            Topic = "AD replication topology",
            FirstSourceId = "INTERNAL-001",
            SecondSourceId = "MICROSOFT-001",
            FirstGuidance = "Approach A",
            SecondGuidance = "Approach B",
            ConflictSummary = "Recommendations differ."
        };

        var result = new KnowledgeRetrievalResult
        {
            Matches = [match],
            Conflicts = [conflict],
            RetrievedUtc = retrievedUtc,
            ConflictAnalysisStatus =
                KnowledgeConflictAnalysisStatus.Completed
        };

        Assert.Single(result.Matches);
        Assert.Single(result.Conflicts);

        Assert.Same(match, Assert.Single(result.Matches));
        Assert.Same(conflict, Assert.Single(result.Conflicts));

        Assert.Equal(retrievedUtc, result.RetrievedUtc);

        Assert.Equal(
            KnowledgeConflictAnalysisStatus.Completed,
            result.ConflictAnalysisStatus);
    }

    [Fact]
    public void KnowledgeRetrievalResult_DefaultsToAnalysisNotPerformed()
    {
        var result = new KnowledgeRetrievalResult
        {
            Matches = [],
            Conflicts = [],
            RetrievedUtc = DateTimeOffset.UtcNow
        };

        Assert.Equal(
            KnowledgeConflictAnalysisStatus.NotPerformed,
            result.ConflictAnalysisStatus);
    }
}