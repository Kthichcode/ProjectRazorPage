using BusinessObjects.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IWalletRepository
    {
        Wallet? GetByUserId(string userId);
        void Add(Wallet wallet);
        void Update(Wallet wallet);
        void Save();
    }
}
