namespace ADOps.Core.Enums;

/// <summary>
/// Identifies the origin of retrieved technical guidance.
/// </summary>
public enum KnowledgeSourceType
{
    Unknown = 0,
    MicrosoftDocumentation,
    MicrosoftSupportCase,
    InternalDocumentation,
    HistoricalIncident,
    ExternalDocumentation
}