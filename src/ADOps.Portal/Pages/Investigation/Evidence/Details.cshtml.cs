using ADOps.Application.Reports;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADOps.Portal.Pages.Investigation.Evidence;

public sealed class DetailsModel : PageModel
{
    private readonly SfoDemoInvestigationProvider _investigationProvider;

    public InvestigationReport? Report { get; private set; }

    public EvidenceReportItem? Evidence { get; private set; }

    public DetailsModel(
        SfoDemoInvestigationProvider investigationProvider)
    {
        _investigationProvider =
            investigationProvider ??
            throw new ArgumentNullException(nameof(investigationProvider));
    }

    public async Task<IActionResult> OnGetAsync(
        string? id,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return NotFound();
        }

        Report =
            await _investigationProvider.GetAsync(
                cancellationToken);

        Evidence =
            Report.Evidence.FirstOrDefault(
                evidence =>
                    string.Equals(
                        evidence.EvidenceId,
                        id,
                        StringComparison.OrdinalIgnoreCase));

        if (Evidence is null)
        {
            return NotFound();
        }

        return Page();
    }
}