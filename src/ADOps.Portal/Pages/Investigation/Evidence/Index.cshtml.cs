using ADOps.Application.Reports;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADOps.Portal.Pages.Investigation.Evidence;

public sealed class IndexModel : PageModel
{
    private readonly SfoDemoInvestigationProvider _investigationProvider;

    public InvestigationReport? Report { get; private set; }

    public IndexModel(
        SfoDemoInvestigationProvider investigationProvider)
    {
        _investigationProvider =
            investigationProvider ??
            throw new ArgumentNullException(nameof(investigationProvider));
    }

    public async Task OnGetAsync(
        CancellationToken cancellationToken)
    {
        Report =
            await _investigationProvider.GetAsync(
                cancellationToken);
    }
}