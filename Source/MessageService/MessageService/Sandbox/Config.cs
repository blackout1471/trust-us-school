using MessageService.Setups.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    internal class Config : ISMTPConfigModel
    {
        public Config() { }

        private string senderAddress;
        private string senderDisplayName;
        private string username;
        private string password;
        private string host;
        private int port;
        private bool enableSSL;
        private bool useDefaultCredentials;
        private bool isBodyHTML;

        public string SenderAddress { get => senderAddress; set => senderAddress = value; }
        public string SenderDisplayName { get => senderDisplayName; set => senderDisplayName = value; }
        public string UserName { get => username; set => username = value; }
        public string Password { get => password; set => password = value; }
        public string Host { get => host; set => host = value; }
        public int Port { get => port; set => port = value; }
        public bool EnableSSL { get => enableSSL; set => enableSSL = value; }
        public bool UseDefaultCredentials { get => useDefaultCredentials; set => useDefaultCredentials = value; }
        public bool IsBodyHTML { get => isBodyHTML; set => isBodyHTML = value; }
    }
}
