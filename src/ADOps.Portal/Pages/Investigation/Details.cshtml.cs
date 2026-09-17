using ADOps.Application.InvestigationDetails;
using ADOps.Portal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADOps.Portal.Pages.Investigation;

public sealed class DetailsModel : PageModel
{
    private readonly SfoDemoInvestigationProvider _investigationProvider;
    private readonly InvestigationDetailService _detailService;

    public InvestigationDetail? Detail { get; private set; }

    public DetailsModel(
        SfoDemoInvestigationProvider investigationProvider,
        InvestigationDetailService detailService)
    {
        _investigationProvider =
            investigationProvider ??
            throw new ArgumentNullException(nameof(investigationProvider));

        _detailService =
            detailService ??
            throw new ArgumentNullException(nameof(detailService));
    }

    public void OnGet()
    {
        var investigation =
            _investigationProvider.GetInvestigation();

        Detail =
            _detailService.Get(investigation);
    }
}