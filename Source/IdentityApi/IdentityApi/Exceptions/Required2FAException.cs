namespace IdentityApi.Exceptions
{
    public class Required2FAException : Exception
    {
        public Required2FAException() : base("Check your email for authetication") { }
    }
}
