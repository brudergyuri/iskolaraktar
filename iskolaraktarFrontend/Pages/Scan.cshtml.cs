using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace iskolaraktarFrontend.Pages
{
    public class ScanModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid QrGuid { get; set; }

        public void OnGet()
        {
        }
    }
}