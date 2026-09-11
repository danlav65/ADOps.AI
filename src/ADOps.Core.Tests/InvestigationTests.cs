using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Core.Tests;

public sealed class InvestigationTests
{
    [Fact]
    public void ChangeStatus_ToRunning_UpdatesStatus()
    {
        var investigation =
            CreateInvestigation();

        investigation.ChangeStatus(
            InvestigationStatus.Running);

        Assert.Equal(
            InvestigationStatus.Running,
            investigation.Status);

        Assert.Null(
            investigation.CompletedUtc);
    }

    [Fact]
    public void ChangeStatus_ToCompleted_UpdatesStatus()
    {
        var investigation =
            CreateInvestigation();

        investigation.ChangeStatus(
            InvestigationStatus.Completed);

        Assert.Equal(
            InvestigationStatus.Completed,
            investigation.Status);

        Assert.Null(
            investigation.CompletedUtc);
    }

    [Fact]
    public void ChangeStatus_ToFailed_UpdatesStatus()
    {
        var investigation =
            CreateInvestigation();

        investigation.ChangeStatus(
            InvestigationStatus.Failed);

        Assert.Equal(
            InvestigationStatus.Failed,
            investigation.Status);

        Assert.Null(
            investigation.CompletedUtc);
    }

    [Fact]
    public void ChangeStatus_ToClosed_UpdatesStatusAndSetsCompletedUtc()
    {
        var investigation =
            CreateInvestigation();

        var before =
            DateTimeOffset.UtcNow;

        investigation.ChangeStatus(
            InvestigationStatus.Closed);

        var after =
            DateTimeOffset.UtcNow;

        Assert.Equal(
            InvestigationStatus.Closed,
            investigation.Status);

        Assert.NotNull(
            investigation.CompletedUtc);

        Assert.InRange(
            investigation.CompletedUtc!.Value,
            before,
            after);
    }

    [Fact]
    public void ChangeStatus_ToCreated_UpdatesStatus()
    {
        var investigation =
            CreateInvestigation();

        investigation.ChangeStatus(
            InvestigationStatus.Running);

        investigation.ChangeStatus(
            InvestigationStatus.Created);

        Assert.Equal(
            InvestigationStatus.Created,
            investigation.Status);

        Assert.Null(
            investigation.CompletedUtc);
    }

    private static Investigation CreateInvestigation()
    {
        return new Investigation
        {
            InvestigationNumber =
                "INV-TEST",

            Incident =
                new Incident
                {
                    IncidentNumber =
                        "INC-TEST",

                    Title =
                        "Test investigation",

                    Environment =
                        "Production",

                    SiteCode =
                        "SFO",

                    DetectedUtc =
                        DateTimeOffset.Parse(
                            "2026-07-09T12:00:00Z")
                },

            StartedUtc =
                DateTimeOffset.Parse(
                    "2026-07-09T12:00:00Z")
        };
    }
}
