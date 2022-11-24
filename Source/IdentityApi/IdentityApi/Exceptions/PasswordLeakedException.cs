namespace IdentityApi.Exceptions
{
    public class PasswordLeakedException : Exception
    {
        public PasswordLeakedException() : base("The password has previously appeared in a data breach." +
            "Please choose a more secure alternative.") { }
    }
}
