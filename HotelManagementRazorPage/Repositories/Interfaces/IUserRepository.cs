using BusinessObjects;
using System.Threading.Tasks;

namespace Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser> GetUserByUsernameAsync(string username);
    }
}
