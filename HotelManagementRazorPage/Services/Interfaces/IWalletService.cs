using BusinessObjects.Entities;
using System.Threading.Tasks;

namespace Services.Interfaces
{
    public interface IWalletService
    {
        Wallet GetUserWallet(string userId);
        void CreateWallet(string userId);
        
        // Return true if deducted successfully, false if not enough balance (though here we might deduct partial)
        // Let's make it simpler: Deduct what is possible and return amount deducted
        decimal DeductBalance(string userId, decimal amountNeeded);
        
        void AddBalance(string userId, decimal amount);
    }
}
