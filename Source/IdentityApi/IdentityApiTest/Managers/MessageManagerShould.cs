using FakeItEasy;
using IdentityApi.Managers;
using MessageService.Messages;
using MessageService.MessageServices;
using MessageService.Providers;
using Microsoft.Extensions.Logging;

namespace IdentityApiUnitTest.Managers
{
    public class MessageManagerShould
    {
        private readonly IMessageService _fakeMessageService;
        private readonly IMessageProvider _fakeMessageProvider;
        private readonly ILogger<MessageManager> _fakeLogger;
        private readonly MessageManager _messageManager;

        public MessageManagerShould()
        {
            _fakeMessageService = A.Fake<IMessageService>();
            _fakeMessageProvider = A.Fake<IMessageProvider>();
            _fakeLogger = A.Fake<ILogger<MessageManager>>();

            _messageManager = new MessageManager(_fakeMessageService, _fakeMessageProvider, _fakeLogger);
        }

        [Fact]
        public async Task ReturnTrue_WhenLoginMessageWasSuccefullySent_SendLoginAttemptMessage()
        {
            // Arrange & Act
            var actual = await _messageManager.SendLoginAttemptMessageAsync("", "");
            //Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task ReturnFalse_WhenLoginMessageWasNotSent_SendLoginAttemptMessage()
        {
            //Arrange
            A.CallTo(() => _fakeMessageService.SendMessageAsync(A<IMessage>.Ignored)).Throws<Exception>();

            //Act
            var actual = await _messageManager.SendLoginAttemptMessageAsync("", "");
            //Assert
            Assert.False(actual);
        }

        [Fact]
        public async Task ReturnTrue_WhenRegistrationMessageWasSuccesfullySent_SendRegistrationMessage()
        {
            //Arrange & Act
            var actual = await _messageManager.SendRegistrationMessageAsync("", "");

            //Assert
            Assert.True(actual);
        }

        [Fact]
        public async Task ReturnFalse_WhenRegistrationMessageWasNotSent_SendRegistrationMessage()
        {
            //Arrange
            A.CallTo(() => _fakeMessageService.SendMessageAsync(A<IMessage>.Ignored)).Throws<Exception>();

            //Act
            var actual = await _messageManager.SendRegistrationMessageAsync("", "");

            //Assert
            Assert.False(actual);

        }
    }
}
