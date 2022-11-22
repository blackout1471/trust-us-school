namespace IdentityApi.Models
{
    public class UserToken
    {
        public int UserID { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
