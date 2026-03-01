using BusinessObjects.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Services.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;

namespace HotelManagementRazorPage.Pages.Bookings
{
    [Authorize]
    public class WalletModel : PageModel
    {
        private readonly IWalletService _walletService;

        public WalletModel(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public Wallet Wallet { get; set; } = null!;
        public List<WalletTransaction> Transactions { get; set; } = new();

        public void OnGet()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
            Wallet = _walletService.GetUserWallet(userId);
            Transactions = _walletService.GetTransactions(userId);
        }
    }
}
