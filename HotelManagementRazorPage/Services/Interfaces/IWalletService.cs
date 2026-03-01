using BusinessObjects.Entities;
using System.Collections.Generic;

namespace Services.Interfaces
{
    public interface IWalletService
    {
        Wallet GetUserWallet(string userId);
        void CreateWallet(string userId);
        decimal DeductBalance(string userId, decimal amountNeeded, string description = "Thanh toán đặt phòng");
        void AddBalance(string userId, decimal amount, string description = "Nạp tiền");
        List<WalletTransaction> GetTransactions(string userId);
    }
}
