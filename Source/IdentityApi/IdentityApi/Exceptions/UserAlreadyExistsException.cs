namespace IdentityApi.Exceptions
{
    public class UserAlreadyExistsException : Exception
    {
        public UserAlreadyExistsException() : base("Email is already in use") { }
    }
}
