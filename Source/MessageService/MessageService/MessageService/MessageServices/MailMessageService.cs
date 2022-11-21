using MessageService.Configurations;
using MessageService.Messages;
using MessageService.Recipients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.MessageServices
{
    public class MailMessageService : IMessageService
    {
        ISMTPConfigModel configuration;
        SmtpClient smtpClient;
        public MailMessageService(ISMTPConfigModel configurations)
        {
            this.configuration = configurations;
            this.Setup();
        }
        public async Task SendMessageAsync(IRecipient recipient, IMessage message)
        {
            if (recipient == null)
                throw new SmtpFailedRecipientException();

            MailMessage mail = CreateMailMessage(message.Message, recipient.To);


            await smtpClient.SendMailAsync(mail);

        }

        private MailMessage CreateMailMessage(string message, string to)
        {
            MailMessage mail = new MailMessage
            {
                From = new MailAddress(configuration.SenderAddress, configuration.SenderDisplayName),
                IsBodyHtml = configuration.IsBodyHTML,
                Body = message
            };
            mail.To.Add(new MailAddress(to));

            return mail;
        }
        private void Setup()
        {
            this.smtpClient = new SmtpClient
            {
                Host = configuration.Host,
                EnableSsl = configuration.EnableSSL,
                Port = configuration.Port,
                UseDefaultCredentials = configuration.UseDefaultCredentials,
                Credentials = new NetworkCredential(configuration.UserName, configuration.Password),
                DeliveryMethod = SmtpDeliveryMethod.Network,
            };
        }

    }

}
