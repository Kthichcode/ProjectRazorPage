using BusinessObjects.Entities;
using Microsoft.EntityFrameworkCore;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Repositories
{
    public class WalletRepository : IWalletRepository
    {
        private readonly AppDbContext _context;

        public WalletRepository(AppDbContext context)
        {
            _context = context;
        }

        public Wallet? GetByUserId(string userId)
        {
            return _context.Wallets.FirstOrDefault(w => w.UserId == userId);
        }

        public void Add(Wallet wallet)
        {
            _context.Wallets.Add(wallet);
        }

        public void Update(Wallet wallet)
        {
            _context.Wallets.Update(wallet);
        }

        public void Save()
        {
            _context.SaveChanges();
        }

        public void AddTransaction(WalletTransaction transaction)
        {
            _context.WalletTransactions.Add(transaction);
        }

        public List<WalletTransaction> GetTransactionsByUserId(string userId)
        {
            return _context.WalletTransactions
                .Include(t => t.Wallet)
                .Where(t => t.Wallet != null && t.Wallet.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }
    }
}
