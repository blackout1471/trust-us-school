namespace IdentityApi.Exceptions
{
    public class SendMessageIssueException : Exception
    {
        public SendMessageIssueException() : base("Something went wrong when sending mail, contact our support.") { }
    }
}
