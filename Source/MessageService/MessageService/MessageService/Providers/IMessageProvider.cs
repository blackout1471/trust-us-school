using MessageService.Messages;

namespace MessageService.Providers
{
    public interface IMessageProvider
    {
        /// <summary>
        /// Gets a IMessage with a predefined message for registers
        /// </summary>
        /// <param name="to"> Recipient of message </param>
        /// <param name="key"> Secret key for otp</param>
        /// <returns></returns>
        public IMessage GetRegisterMessage(string to, string key);

        /// <summary>
        /// Returns a IMessage with predefined message for login attempts.
        /// </summary>
        /// <param name="to"> Recipient of message </param>
        /// <param name="otp"> One Time Password</param>
        public IMessage GetLoginAttemptMessage(string to, string otp);
    }
}
