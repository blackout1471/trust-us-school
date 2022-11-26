namespace MessageService.Messages
{
    public class EmailMessage : IMessage
    {
        private string message;
        private string to;

        public string Message { get => message; set => message = value; }
        public string To { get => to; set => to = value; }
    }
}
