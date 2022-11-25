using MessageService.Configurations;
using MessageService.Messages;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace MessageService.MessageServices
{
    public class MailMessageService : IMessageService
    {
        SMTPConfigModel configuration;
        SmtpClient smtpClient;
        public MailMessageService(IOptions<SMTPConfigModel> configurations)
        {
            this.configuration = configurations.Value;
            this.SetupSmtpClient();
        }

        /// <inheritdoc/>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SendMessageAsync(IMessage message)
        {
            if (string.IsNullOrEmpty(message.To))
                throw new ArgumentNullException();

            MailMessage mail = CreateMailMessage(message.Message, message.To);

            await smtpClient.SendMailAsync(mail);
        }
        /// <summary>
        /// Creates a MailMessage object
        /// </summary>
        /// <param name="message"> The message that is to be sent </param>
        /// <param name="to"> The recipient of the mail </param>
        /// <returns>A new MailMessage object</returns>
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

        /// <summary>
        /// Sets up smtp client from local configuration
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        private void SetupSmtpClient()
        {
            try
            {
                this.smtpClient = new SmtpClient
                {
                    Host = configuration.Host,
                    EnableSsl = configuration.EnableSSL,
                    Port = configuration.Port,
                    UseDefaultCredentials = configuration.UseDefaultCredentials,
                    Credentials = configuration.UseDefaultCredentials ? null : new NetworkCredential(configuration.UserName, configuration.Password),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                };

            }
            catch (Exception)
            {

                throw new ArgumentException();
            }
        }

    }

}
