using MessageService.Messages;
using MessageService.Recipients;

namespace MessageService.MessageServices
{
    public interface IMessageService
    {
        public Task SendMessageAsync(IRecipient recipient, IMessage message);

    }
}
