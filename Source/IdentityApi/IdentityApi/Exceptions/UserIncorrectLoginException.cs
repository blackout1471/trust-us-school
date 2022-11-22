namespace IdentityApi.Exceptions
{
    public class UserIncorrectLoginException : Exception
    {
        public UserIncorrectLoginException() : base("No account with the given credentials exists") { }
    }
}
