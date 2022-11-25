using FakeItEasy;
using MessageService.Configurations;
using MessageService.Messages;
using MessageService.MessageServices;
using Microsoft.Extensions.Options;

namespace MessageServiceUnitTest
{
    public class MailMessageServiceShould
    {
        [Fact]
        public void ThrowArgumentException_WhenMisconfigured_Constructor()
        {
            //Arrange
            var fakeConfig = A.Fake<IOptions<SMTPConfigModel>>();
            fakeConfig.Value.Host = null;
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
            var options = Options.Create(GenerateFakeSMTPConfig());
            var mailMessageService = new MailMessageService(options);
            message.Message = "Hello there";
            //Act
            var func = async () => await mailMessageService.SendMessageAsync(message);
            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(func);
        }
  

        private static SMTPConfigModel GenerateFakeSMTPConfig()
        {
            var fakeConfig = new SMTPConfigModel();
            fakeConfig.UseDefaultCredentials = false;
            fakeConfig.SenderDisplayName = "Test";
            fakeConfig.IsBodyHTML = false;
            fakeConfig.EnableSSL = true;
            fakeConfig.Host = "smtp.gmail.com";
            fakeConfig.Port = 587;
            fakeConfig.UserName = "test@gmail.com";
            fakeConfig.SenderAddress = "test@gmail.com";
            fakeConfig.Password = "password";

            return fakeConfig;

        }
    }
}