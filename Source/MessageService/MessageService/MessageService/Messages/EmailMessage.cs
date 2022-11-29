namespace MessageService.Messages
{
    internal class EmailMessage : IMessage
    {
        public string Message { get; set; }
        public string To { get; set; }
    }
}
