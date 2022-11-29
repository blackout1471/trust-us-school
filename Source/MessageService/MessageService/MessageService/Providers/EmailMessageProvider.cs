using MessageService.Messages;

namespace MessageService.Providers
{
    public class EmailMessageProvider : IMessageProvider
    {
        /// <inheritdoc/>
        public IMessage GetLoginAttemptMessage(string to, string otp)
        {
            var message = "There has been a login attempt from a new location, this is your one-time password for this login:\n" + otp
            + "\nIf this was not you, consider changing your password.";

            return new EmailMessage()
            {
                To = to,
                Message = message
            };
        }

        /// <inheritdoc/>
        public IMessage GetRegisterMessage(string to, string key)
        {
            var message = "Welcome, this is your 2-factor password:\n" + key + "\nSave this password in case you lose access to your email.\nUse this key to verify registration.";


            return new EmailMessage { To = to, Message = message };

        }
    }
}
