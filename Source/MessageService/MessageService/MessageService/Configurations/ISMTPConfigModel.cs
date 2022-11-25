namespace MessageService.Configurations
{
    public interface ISMTPConfigModel
    {
        public string SenderAddress { get; }
        public string SenderDisplayName { get;}
        public string UserName { get; }
        public string Password { get; }
        public string Host { get; }
        public int Port { get;}
        public bool EnableSSL { get; }
        public bool UseDefaultCredentials { get; }
        public bool IsBodyHTML { get; }
    }
}
