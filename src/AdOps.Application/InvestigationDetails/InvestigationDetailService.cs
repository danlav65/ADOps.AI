using ADOps.Core.Entities;

namespace ADOps.Application.InvestigationDetails;

public sealed class InvestigationDetailService
{
    public InvestigationDetail Get(
        ADOps.Core.Entities.Investigation investigation)
    {
        ArgumentNullException.ThrowIfNull(investigation);

        return new InvestigationDetail
        {
            InvestigationNumber =
                investigation.InvestigationNumber,

            Status =
                investigation.Status,

            StartedUtc =
                investigation.StartedUtc,

            CompletedUtc =
                investigation.CompletedUtc,

            IncidentNumber =
                investigation.Incident.IncidentNumber,

            Title =
                investigation.Incident.Title,

            Description =
                investigation.Incident.Description,

            Environment =
                investigation.Incident.Environment,

            SiteCode =
                investigation.Incident.SiteCode,

            Severity =
                investigation.Incident.Severity,

            IncidentStatus =
                investigation.Incident.Status,

            DetectedUtc =
                investigation.Incident.DetectedUtc,

            ResolvedUtc =
                investigation.Incident.ResolvedUtc
        };
    }
}