namespace ADOps.Application.Reports;

public sealed class SupportingKnowledgeItem
{
    public required string Source { get; init; }

    public required string Description { get; init; }

    public string? SourceType { get; init; }

    public string? SourceUri { get; init; }
}