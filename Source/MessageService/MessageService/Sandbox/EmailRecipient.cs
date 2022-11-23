using MessageService.Recipients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sandbox
{
    internal class EmailRecipient : IRecipient
    {
        private string to;

        public string To { get => to; set => to = value; }
    }
}
