using IdentityApi.Interfaces;
using MessageService.MessageServices;
using MessageService.Providers;
using Microsoft.AspNetCore.DataProtection.KeyManagement;

namespace IdentityApi.Managers
{
    public class MessageManager : IMessageManager
    {
        private readonly IMessageService _messageService;
        private readonly IMessageProvider _messageProvider;

        public MessageManager(IMessageService messageService, IMessageProvider messageProvider)
        {
            _messageService = messageService;
            _messageProvider = messageProvider;
        }

        /// <inheritdoc/>
        public void SendLoginAttemptMessage(string to, string otp)
        {
            var message = _messageProvider.GetLoginAttemptMessage(to, otp);
            _messageService.SendMessageAsync(message).Wait();
        }

        /// <inheritdoc/>
        public void SendRegistrationMessage(string to, string key)
        {
            var message = _messageProvider.GetRegisterMessage(to,key);
            _messageService.SendMessageAsync(message).Wait();
        }
    }
}
