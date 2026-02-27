using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;

namespace HotelManagementRazorPage.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly IAccountService _accountService;

        public LogoutModel(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            await _accountService.LogoutAsync();
            return Redirect(returnUrl ?? Url.Content("~/"));
        }
    }
}
