namespace IdentityApi.Exceptions
{
    public class AccountIsNotVerifiedException : Exception
    {
        public AccountIsNotVerifiedException() : base("User is not verified, check e-mail") {}
    }
}
