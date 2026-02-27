using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<LoginModel> _logger;

        public LoginModel(IAccountService accountService, ILogger<LoginModel> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }
        //gg

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public string? ReturnUrl { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
            [Display(Name = "Tên đăng nhập")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Ghi nhớ đăng nhập")]
            public bool RememberMe { get; set; }
        }

        public void OnGet(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
        {
            ReturnUrl = returnUrl ?? Url.Content("~/");

            if (ModelState.IsValid)
            {
                var result = await _accountService.LoginAsync(Input.UserName, Input.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User {UserName} logged in.", Input.UserName);
                    return LocalRedirect(ReturnUrl);
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không đúng.");
            }

            return Page();
        }
    }
}
