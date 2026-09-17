using ADOps.Portal.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ADOps.Portal.Pages.Investigation;

public sealed class SfoDemoModel : PageModel
{
    private readonly SfoDemoInvestigationProvider _investigationProvider;

    public SfoP1DemoContext Demo { get; private set; } = null!;

    public SfoDemoModel(
        SfoDemoInvestigationProvider investigationProvider)
    {
        _investigationProvider =
            investigationProvider ??
            throw new ArgumentNullException(nameof(investigationProvider));
    }

    public void OnGet()
    {
        Demo =
            _investigationProvider.GetDemoContext();
    }
}