namespace ADOps.Core.Enums;

/// <summary>
/// Defines the category of correlation identified during analysis.
/// </summary>
public enum CorrelationCategory
{
    /// <summary>
    /// Category has not been determined.
    /// </summary>
    Unknown,

    /// <summary>
    /// Active Directory replication-related correlation.
    /// </summary>
    Replication,

    /// <summary>
    /// Authentication-related correlation.
    /// </summary>
    Authentication,

    /// <summary>
    /// DNS-related correlation.
    /// </summary>
    DNS,

    /// <summary>
    /// Kerberos-related correlation.
    /// </summary>
    Kerberos,

    /// <summary>
    /// Group Policy-related correlation.
    /// </summary>
    GroupPolicy,

    /// <summary>
    /// Network-related correlation.
    /// </summary>
    Network,

    /// <summary>
    /// Performance-related correlation.
    /// </summary>
    Performance
}