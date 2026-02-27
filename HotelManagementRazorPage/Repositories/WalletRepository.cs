using BusinessObjects.Entities;
using Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
