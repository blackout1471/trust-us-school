using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MessageService.Messages
{
    public interface IMessage
    {
        string Message { get; set; }
    }
}
