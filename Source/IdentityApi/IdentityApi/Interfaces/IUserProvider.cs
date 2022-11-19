using IdentityApi.DbModels;
using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserProvider
    {
        Task<DbUser> CreateUser(DbUser userCreate);
        Task<DbUser> GetUserByEmail(string email);
        Task<DbUser> GetUserByID(int id);
    }
}
