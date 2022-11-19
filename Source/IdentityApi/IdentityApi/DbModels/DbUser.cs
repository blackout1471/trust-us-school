namespace IdentityApi.DbModels
{
    public class DbUser
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string HashedPassword { get; set; }
        public string Salt { get; set; }
    }
}
