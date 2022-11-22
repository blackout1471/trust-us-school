using MessageService.Configurations;
using MessageService.Messages;
using MessageService.Recipients;
using System.Net;
using System.Net.Mail;

namespace MessageService.MessageServices
{
    public class MailMessageService : IMessageService
    {
        ISMTPConfigModel configuration;
        SmtpClient smtpClient;
        public MailMessageService(ISMTPConfigModel configurations)
        {
            this.configuration = configurations;
            this.SetupSmtpClient();
        }

        /// <summary>
        /// Sends a message async
        /// </summary>
        /// <param name="recipient"> Recipient of message </param>
        /// <param name="message"> Message to be sent</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task SendMessageAsync(IRecipient recipient, IMessage message)
        {
            if (string.IsNullOrEmpty(recipient.To))
                throw new ArgumentNullException();

            MailMessage mail = CreateMailMessage(message.Message, recipient.To);

            await smtpClient.SendMailAsync(mail);
        }
        /// <summary>
        /// Creates a MailMessage object
        /// </summary>
        /// <param name="message"> The message that is to be sent </param>
        /// <param name="to"> The recipient of the mail </param>
        /// <returns></returns>
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
                    Credentials = new NetworkCredential(configuration.UserName, configuration.Password),
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
