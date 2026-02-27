using BusinessObjects.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using System;

namespace Services
{
    public class WalletService : IWalletService
    {
        private readonly IWalletRepository _walletRepo;

        public WalletService(IWalletRepository walletRepo)
        {
            _walletRepo = walletRepo;
        }

        public void CreateWallet(string userId)
        {
            if (_walletRepo.GetByUserId(userId) == null)
            {
                var wallet = new Wallet
                {
                    UserId = userId,
                    Balance = 0
                };
                _walletRepo.Add(wallet);
                _walletRepo.Save();
            }
        }

        public Wallet GetUserWallet(string userId)
        {
            var wallet = _walletRepo.GetByUserId(userId);
            if (wallet == null)
            {
                CreateWallet(userId);
                wallet = _walletRepo.GetByUserId(userId);
            }
            return wallet;
        }

        public decimal DeductBalance(string userId, decimal amountNeeded)
        {
            if (amountNeeded < 0) throw new ArgumentException("Amount cannot be negative");

            var wallet = GetUserWallet(userId);
            
            decimal deducted = 0;
            if (wallet.Balance >= amountNeeded)
            {
                wallet.Balance -= amountNeeded;
                deducted = amountNeeded;
            }
            else
            {
                deducted = wallet.Balance;
                wallet.Balance = 0;
            }

            _walletRepo.Update(wallet);
            _walletRepo.Save();
            
            return deducted;
        }

        public void AddBalance(string userId, decimal amount)
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative");
            
            var wallet = GetUserWallet(userId);
            wallet.Balance += amount;
            _walletRepo.Update(wallet);
            _walletRepo.Save();
        }
    }
}
