// See https://aka.ms/new-console-template for more information
using MessageService.MessageServices;
using Sandbox;

Config config = new Config
{
    IsBodyHTML = false,
    EnableSSL = true,
    Host = "smtp.gmail.com",
    Port = 587,
    UserName = "test@gmail.com",
    Password= "password",
    SenderAddress = "test@gmail.com",
    SenderDisplayName = "Trust Us",
    UseDefaultCredentials = false
};
MailMessageService a = new MailMessageService(config);
EmailRecipient emailRecipient = new EmailRecipient { To = "renelorentzen@hotmail.com" };
EmailMessage emailMessage = new EmailMessage { Message = "Hello there" };
a.SendMessageAsync(emailRecipient, emailMessage);
Console.WriteLine("Hello there");
