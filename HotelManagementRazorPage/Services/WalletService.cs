using BusinessObjects.Entities;
using Repositories.Interfaces;
using Services.Interfaces;
using System;
using System.Collections.Generic;

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
            if (_walletRepo.GetByUserId(userId) != null) return;
            try
            {
                var wallet = new Wallet { UserId = userId, Balance = 0 };
                _walletRepo.Add(wallet);
                _walletRepo.Save();
            }
            catch (Exception ex) when (
                ex is Microsoft.EntityFrameworkCore.DbUpdateException ||
                ex.InnerException?.Message.Contains("FOREIGN KEY") == true)
            {
                // UserId không tồn tại trong bảng Users → bỏ qua
            }
        }

        public Wallet? GetUserWallet(string userId)
        {
            var wallet = _walletRepo.GetByUserId(userId);
            if (wallet == null)
            {
                CreateWallet(userId);
                wallet = _walletRepo.GetByUserId(userId);
            }
            return wallet;
        }

        public decimal DeductBalance(string userId, decimal amountNeeded, string description = "Thanh toán đặt phòng")
        {
            if (amountNeeded < 0) throw new ArgumentException("Amount cannot be negative");

            var wallet = GetUserWallet(userId);
            if (wallet == null) return 0; // user không tồn tại trong DB
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

            if (deducted > 0)
            {
                var transaction = new WalletTransaction
                {
                    WalletId = wallet.Id,
                    Amount = deducted,
                    Type = WalletTransactionType.Payment,
                    Description = description,
                    CreatedAt = DateTime.UtcNow
                };
                _walletRepo.AddTransaction(transaction);
            }

            _walletRepo.Save();

            return deducted;
        }

        public void AddBalance(string userId, decimal amount, string description = "Nạp tiền")
        {
            if (amount < 0) throw new ArgumentException("Amount cannot be negative");

            var wallet = GetUserWallet(userId);
            if (wallet == null) return; // user không tồn tại trong DB
            wallet.Balance += amount;
            _walletRepo.Update(wallet);

            // Log transaction
            var transaction = new WalletTransaction
            {
                WalletId = wallet.Id,
                Amount = amount,
                Type = WalletTransactionType.Refund,
                Description = description,
                CreatedAt = DateTime.UtcNow
            };
            _walletRepo.AddTransaction(transaction);
            _walletRepo.Save();
        }

        public List<WalletTransaction> GetTransactions(string userId)
        {
            return _walletRepo.GetTransactionsByUserId(userId);
        }
    }
}
