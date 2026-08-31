namespace ADOps.Core.Entities;

public sealed class SystemInfoRecord
{
    public required string DomainController { get; init; }

    public required string Site { get; init; }

    public required string ComputerName { get; init; }

    public required string OperatingSystem { get; init; }

    public required string OsVersion { get; init; }

    public required string BuildNumber { get; init; }

    public required string Edition { get; init; }

    public required string Architecture { get; init; }

    public DateTimeOffset? InstallDate { get; init; }

    public DateTimeOffset? LastBootTime { get; init; }

    public required string TimeZone { get; init; }

    public int LogicalProcessors { get; init; }

    public int PhysicalMemoryGb { get; init; }

    public double SystemDriveFreeSpaceGb { get; init; }

    public bool VirtualMachine { get; init; }

    public string? Hypervisor { get; init; }

    public required string PowerShellVersion { get; init; }

    public required string DotNetVersion { get; init; }

    public DateTimeOffset CollectedUtc { get; init; }
}