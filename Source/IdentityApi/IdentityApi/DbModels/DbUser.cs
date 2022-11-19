using IdentityApi.Models;

namespace IdentityApi.DbModels
{
    public class DbUser : User
    {
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
    }
}
