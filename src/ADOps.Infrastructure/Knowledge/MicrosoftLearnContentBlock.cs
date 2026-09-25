namespace ADOps.Infrastructure.Knowledge;

/// <summary>
/// Represents one structured block of extracted article content.
/// </summary>
public sealed class MicrosoftLearnContentBlock
{
    public required MicrosoftLearnContentBlockType Type { get; init; }

    public required string Content { get; init; }

    public required int Sequence { get; init; }
}