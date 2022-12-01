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
        private readonly ILogger<MessageManager> _logger;

        public MessageManager(IMessageService messageService, IMessageProvider messageProvider, ILogger<MessageManager> logger)
        {
            _messageService = messageService;
            _messageProvider = messageProvider;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> SendLoginAttemptMessageAsync(string to, string otp)
        {
            var message = _messageProvider.GetLoginAttemptMessage(to, otp);
            try
            {
                await _messageService.SendMessageAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong when sending message");
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> SendRegistrationMessageAsync(string to, string key)
        {
            var message = _messageProvider.GetRegisterMessage(to, key);
            try
            {
                await _messageService.SendMessageAsync(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Something went wrong when sending message");
                return false;
            }
        }
    }
}
