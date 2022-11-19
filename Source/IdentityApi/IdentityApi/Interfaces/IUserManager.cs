using IdentityApi.Models;

namespace IdentityApi.Interfaces
{
    public interface IUserManager
    {
        Task<User> CreateUser(UserCreate userCreate);
        Task<User> Login(UserLogin userLogin);
    }
}
