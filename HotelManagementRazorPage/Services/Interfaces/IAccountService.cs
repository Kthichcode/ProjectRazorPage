using BusinessObjects;
using Microsoft.AspNetCore.Identity;

namespace Services.Interfaces
{
    public interface IAccountService
    {
        Task<SignInResult> LoginAsync(string username, string password);
        Task<IdentityResult> RegisterAsync(ApplicationUser user, string password);
        Task LogoutAsync();
    }
}
