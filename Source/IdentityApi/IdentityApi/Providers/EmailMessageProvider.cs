using IdentityApi.Interfaces;
using IdentityApi.Messages;
using MessageService.Messages;
using static System.Net.WebRequestMethods;

namespace IdentityApi.Providers
{
    public class EmailMessageProvider : IMessageProvider
    {
        /// <inheritdoc/>
        public IMessage GetLoginAttemptMessage(string to, string otp)
        {
            var message = "There has been a login attempt from a new location, this is your one-time password for this login: " + otp
            + " \n If this was not you, consider changing your password.";

            return new EmailMessage()
            {
                To = to,
                Message = message
            };
        }

        /// <inheritdoc/>
        public IMessage GetRegisterMessage(string to, string key)
        {
            var message = "Welcome, this is your 2-factor password:" + key + " Save this password in case you lose access to your email.";


            return new EmailMessage { To = to, Message = message };

        }
    }
}
