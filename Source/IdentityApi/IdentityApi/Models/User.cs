namespace IdentityApi.Models
{
    public class User : UserBase
    {
        public int ID { get; set; }
        public bool IsVerified { get; set; }
        public bool IsLocked { get; set; }
        public int FailedTries { get; set; }
        public DateTime? LockedDate { get; set; }
    }
}
