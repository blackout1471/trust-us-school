namespace IdentityApi.Models
{
    public class UserLocation
    {
        public int ID { get; set; }
        public string UserID { get; set; }
        public byte[] IP { get; set; }
        public string UserAgent { get; set; }

    }
}
