using FakeItEasy;
using MessageService.Configurations;
using MessageService.Messages;
using MessageService.MessageServices;
using MessageService.Recipients;
using System.Net.Mail;

namespace MessageServiceUnitTest
{
    public class MailMessageServiceShould
    {
        [Fact]
        public void ThrowArgumentException_WhenMisconfigured_Constructor()
        {
            //Arrange
            var fakeConfig = A.Fake<ISMTPConfigModel>();
            fakeConfig.Host = null;
            //Act
            var func = () => new MailMessageService(fakeConfig);
            //Assert
            Assert.Throws<ArgumentException>(func);
        }

        [Fact]
        public async void ThrowsArgumentNullException_WhenRecipientToIsNullOrEmpty_SendMessageAsync()
        {
            //Arrange
            var message = A.Fake<IMessage>();
            var recipient = A.Fake<IRecipient>();
            var fakeConfig = A.Fake<ISMTPConfigModel>();
            fakeConfig.UseDefaultCredentials = false;
            fakeConfig.SenderDisplayName = "Test";
            fakeConfig.IsBodyHTML = false;
            fakeConfig.EnableSSL = true;
            fakeConfig.Host = "smtp.gmail.com";
            fakeConfig.Port = 587;
            fakeConfig.UserName = "test@gmail.com";
            fakeConfig.SenderAddress = "test@gmail.com";
            message.Message = "Hello there";
            var mailMessageService = new MailMessageService(fakeConfig);
            //Act
            var func = async () => await mailMessageService.SendMessageAsync(recipient, message);
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(func);
        }
        [Fact]
        public async void ThrowsSmtpException_WhenSenderIsUnauthorized_SendMessageAsync()
        {
            //Arrange
            var message = A.Fake<IMessage>();
            var recipient = A.Fake<IRecipient>();
            var fakeConfig = A.Fake<ISMTPConfigModel>();

            fakeConfig.UseDefaultCredentials = false;
            fakeConfig.SenderDisplayName = "Test";
            fakeConfig.IsBodyHTML = false;
            fakeConfig.EnableSSL = true;
            fakeConfig.Host = "smtp.gmail.com";
            fakeConfig.Port = 587;
            fakeConfig.UserName = "test@gmail.com";
            fakeConfig.SenderAddress = "test@gmail.com";
            fakeConfig.Password = "password";

            message.Message = "Hello there";
            recipient.To = "test@gmail.com";
            var mailMessageService = new MailMessageService(fakeConfig);
            //Act
            var func = async () => await mailMessageService.SendMessageAsync(recipient, message);
            //Assert
            await Assert.ThrowsAsync<SmtpException>(func);
        }
    }
}