using BusinessObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace HotelManagementRazorPage.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<RegisterModel> _logger;

        public RegisterModel(IAccountService accountService, ILogger<RegisterModel> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public class InputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
            [Display(Name = "Họ và tên")]
            [StringLength(100, ErrorMessage = "Họ và tên không được vượt quá 100 ký tự.")]
            public string FullName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
            [Display(Name = "Tên đăng nhập")]
            [StringLength(50, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải từ 3 đến 50 ký tự.")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập email.")]
            [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
            [Display(Name = "Số điện thoại")]
            public string? PhoneNumber { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Mật khẩu")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]
            public string Password { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu.")]
            [DataType(DataType.Password)]
            [Display(Name = "Xác nhận mật khẩu")]
            [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = new ApplicationUser
            {
                FullName = Input.FullName,
                UserName = Input.UserName,
                Email = Input.Email,
                PhoneNumber = Input.PhoneNumber
            };

            var result = await _accountService.RegisterAsync(user, Input.Password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {UserName} registered successfully.", Input.UserName);
                return RedirectToPage("/Account/Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return Page();
        }
    }
}
