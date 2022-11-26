using MessageService.Messages;

namespace MessageService.MessageServices
{
    public interface IMessageService
    {
        /// <summary>
        /// Sends a message async
        /// </summary>
        /// <param name="message"> Message to be sent</param>
        public Task SendMessageAsync(IMessage message);

    }
}
