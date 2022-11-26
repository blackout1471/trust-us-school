using IdentityApi.Models;

namespace IdentityApi.DbModels
{
    public class DbUser : User
    {
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
        public long Counter { get; set; }
        public string SecretKey { get; set; }
        public DateTime? LastRequestDate { get; set; }
    }
}
