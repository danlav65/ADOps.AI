using ADOps.Core.Enums;

namespace ADOps.Core.Entities;

/// <summary>
/// Represents the business and operational impact of a technical condition.
/// </summary>
public sealed class OperationalRisk : EntityBase
{
    public required string Title { get; init; }

    public required string Description { get; init; }

    public RiskLevel Level { get; private set; }

    public required string BusinessImpact { get; init; }

    public required string TechnicalReason { get; init; }

    public int Score { get; private set; }

    public DateTimeOffset EvaluatedUtc { get; private set; }


    public void Evaluate(
        RiskLevel level,
        int score)
    {
        Level = level;
        Score = score;
        EvaluatedUtc = DateTimeOffset.UtcNow;

        Touch();
    }
}