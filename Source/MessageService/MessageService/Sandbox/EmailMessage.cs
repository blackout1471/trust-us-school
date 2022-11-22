using MessageService.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    internal class EmailMessage : IMessage
    {
        private string message;

        public string Message { get => message; set => message = value; }
    }
}
