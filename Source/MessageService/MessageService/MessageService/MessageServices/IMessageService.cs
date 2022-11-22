using MessageService.Messages;
using MessageService.Recipients;

namespace MessageService.MessageServices
{
    public interface IMessageService
    {
        /// <summary>
        /// Sends a message async
        /// </summary>
        /// <param name="recipient"> Recipient of message </param>
        /// <param name="message"> Message to be sent</param>
        public Task SendMessageAsync(IRecipient recipient, IMessage message);

    }
}
