using ADOps.Application.InvestigationDetails;
using ADOps.Core.Entities;
using ADOps.Core.Enums;

namespace ADOps.Application.Tests;

public sealed class InvestigationDetailServiceTests
{
    [Fact]
    public void Get_MapsInvestigationMetadata()
    {
        var startedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(startedUtc);

        var service =
            new InvestigationDetailService();

        var detail =
            service.Get(investigation);

        Assert.Equal(
            investigation.InvestigationNumber,
            detail.InvestigationNumber);

        Assert.Equal(
            investigation.Status,
            detail.Status);

        Assert.Equal(
            investigation.StartedUtc,
            detail.StartedUtc);

        Assert.Equal(
            investigation.CompletedUtc,
            detail.CompletedUtc);
    }

    [Fact]
    public void Get_MapsIncidentMetadata()
    {
        var detectedUtc =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(detectedUtc);

        var service =
            new InvestigationDetailService();

        var detail =
            service.Get(investigation);

        var incident =
            investigation.Incident;

        Assert.Equal(
            incident.IncidentNumber,
            detail.IncidentNumber);

        Assert.Equal(
            incident.Title,
            detail.Title);

        Assert.Equal(
            incident.Description,
            detail.Description);

        Assert.Equal(
            incident.Environment,
            detail.Environment);

        Assert.Equal(
            incident.SiteCode,
            detail.SiteCode);

        Assert.Equal(
            incident.Severity,
            detail.Severity);

        Assert.Equal(
            incident.Status,
            detail.IncidentStatus);

        Assert.Equal(
            incident.DetectedUtc,
            detail.DetectedUtc);

        Assert.Equal(
            incident.ResolvedUtc,
            detail.ResolvedUtc);
    }

    [Fact]
    public void Get_PreservesNullLifecycleDates()
    {
        var timestamp =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(timestamp);

        var service =
            new InvestigationDetailService();

        var detail =
            service.Get(investigation);

        Assert.Null(
            investigation.CompletedUtc);

        Assert.Null(
            investigation.Incident.ResolvedUtc);

        Assert.Null(
            detail.CompletedUtc);

        Assert.Null(
            detail.ResolvedUtc);
    }

    [Fact]
    public void Get_MapsCompletedAndResolvedDates()
    {
        var timestamp =
            new DateTimeOffset(
                2026,
                7,
                9,
                12,
                0,
                0,
                TimeSpan.Zero);

        var investigation =
            CreateInvestigation(timestamp);

        investigation.ChangeStatus(
            InvestigationStatus.Closed);

        investigation.Incident.ChangeStatus(
            IncidentStatus.Resolved);

        var service =
            new InvestigationDetailService();

        var detail =
            service.Get(investigation);

        Assert.NotNull(
            detail.CompletedUtc);

        Assert.NotNull(
            detail.ResolvedUtc);

        Assert.Equal(
            investigation.CompletedUtc,
            detail.CompletedUtc);

        Assert.Equal(
            investigation.Incident.ResolvedUtc,
            detail.ResolvedUtc);
    }

    [Fact]
    public void Get_Throws_WhenInvestigationIsNull()
    {
        var service =
            new InvestigationDetailService();

        Assert.Throws<ArgumentNullException>(
            () => service.Get(null!));
    }

    private static ADOps.Core.Entities.Investigation CreateInvestigation(
        DateTimeOffset timestamp)
    {
        var incident =
            new Incident
            {
                IncidentNumber = "INC-SFO-20260709",
                Title = "AD replication failure",
                Environment = "Production",
                SiteCode = "SFO",
                DetectedUtc = timestamp
            };

        incident.UpdateDescription(
            "P1 Active Directory replication incident at SFO");

        incident.UpdateSeverity(
            SeverityLevel.Critical);

        incident.ChangeStatus(
            IncidentStatus.Investigating);

        var investigation =
            new ADOps.Core.Entities.Investigation
            {
                InvestigationNumber = "INV-SFO-20260709",
                Incident = incident,
                StartedUtc = timestamp
            };

        investigation.ChangeStatus(
            InvestigationStatus.Running);

        return investigation;
    }
}
