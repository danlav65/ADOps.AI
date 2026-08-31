namespace ADOps.Core.Enums;

/// <summary>
/// Represents the lifecycle state of an AD investigation.
/// </summary>
public enum InvestigationStatus
{
    /// <summary>
    /// Investigation has been created but not started.
    /// </summary>
    Created,

    /// <summary>
    /// Investigation is currently being executed.
    /// </summary>
    Running,

    /// <summary>
    /// Investigation completed successfully.
    /// </summary>
    Completed,

    /// <summary>
    /// Investigation failed during execution.
    /// </summary>
    Failed,

    /// <summary>
    /// Investigation has been closed.
    /// </summary>
    Closed
}