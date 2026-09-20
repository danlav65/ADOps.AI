using ADOps.Core.Entities;

namespace ADOps.Core.Tests;

public sealed class KnowledgeConflictTests
{
    [Fact]
    public void KnowledgeConflict_PreservesBothSourceReferences()
    {
        var conflict = new KnowledgeConflict
        {
            Topic = "Active Directory replication topology",

            FirstSourceId = "INTERNAL-AD-001",

            SecondSourceId = "MS-SUPPORT-001",

            FirstGuidance = "Approach A",

            SecondGuidance = "Approach B",

            ConflictSummary =
                "The sources recommend different approaches."
        };

        Assert.Equal(
            "INTERNAL-AD-001",
            conflict.FirstSourceId);

        Assert.Equal(
            "MS-SUPPORT-001",
            conflict.SecondSourceId);

        Assert.NotEqual(
            conflict.FirstSourceId,
            conflict.SecondSourceId);
    }

    [Fact]
    public void KnowledgeConflict_PreservesBothRecommendations()
    {
        var conflict = new KnowledgeConflict
        {
            Topic = "Active Directory replication topology",

            FirstSourceId = "INTERNAL-AD-001",

            SecondSourceId = "MS-SUPPORT-001",

            FirstGuidance = "Approach A",

            SecondGuidance = "Approach B",

            ConflictSummary =
                "The sources recommend different approaches."
        };

        Assert.Equal(
            "Approach A",
            conflict.FirstGuidance);

        Assert.Equal(
            "Approach B",
            conflict.SecondGuidance);

        Assert.Equal(
            "The sources recommend different approaches.",
            conflict.ConflictSummary);
    }
}