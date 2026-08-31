namespace ADOps.Core.Entities.Reporting;

public sealed class ReportNormalizationResult<T>
{
    public required string ReportId { get; init; }

    public required ReportType ReportType { get; init; }

    public bool Succeeded { get; init; }

    public T? Record { get; init; }

    public IReadOnlyCollection<string> Errors { get; init; }
        = [];
}