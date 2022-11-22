namespace IdentityApi.Models
{
    public class UserLocation
    {
        public int ID { get; set; }
        public int UserID { get; set; }
        public string IP { get; set; }
        public string UserAgent { get; set; }
        public bool Successful { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
