namespace IdentityApi.Exceptions
{
    public class AccountLockedException : Exception
    {
        public AccountLockedException() : base("The specified account is locked") { }
    }
}
